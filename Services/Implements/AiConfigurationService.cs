using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.AI;
using Services.Interfaces;

namespace Services.Implements;

public sealed class AiConfigurationService : IAiConfigurationService
{
    private const decimal DefaultTemperature = 0.2m;
    private const int DefaultMaxOutputTokens = 900;
    private const int DefaultTimeoutSeconds = 45;

    private readonly IAiProviderSettingRepository _repository;
    private readonly AiProviderClientFactory _clientFactory;
    private readonly IUserActivityService _userActivityService;

    public AiConfigurationService(
        IAiProviderSettingRepository? repository = null,
        AiProviderClientFactory? clientFactory = null,
        IUserActivityService? userActivityService = null)
    {
        _repository = repository ?? new AiProviderSettingRepository();
        _clientFactory = clientFactory ?? new AiProviderClientFactory();
        _userActivityService = userActivityService ?? new UserActivityService();
    }

    public async Task<ServiceResult<List<AiProviderSettingDto>>> GetSettingsAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureAdmin<List<AiProviderSettingDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var savedSettings = await _repository.GetAllAsync(cancellationToken);
            var result = Enum.GetValues<AiProviderName>()
                .Select(provider =>
                {
                    var saved = savedSettings.FirstOrDefault(item => item.ProviderName == provider);
                    return saved is null ? CreateDefaultDto(provider) : MapToDto(saved);
                })
                .ToList();

            return ServiceResult<List<AiProviderSettingDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<AiProviderSettingDto>>.Failure(ErrorMessages.SystemError, ex.Message);
        }
    }

    public async Task<ServiceResult<AiProviderSettingDto>> SaveSettingAsync(
        SaveAiProviderSettingRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorizationResult = EnsureAdmin<AiProviderSettingDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var validationErrors = ValidateRequest(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<AiProviderSettingDto>.Failure(
                ErrorMessages.ValidationFailed,
                validationErrors.ToArray());
        }

        try
        {
            var existing = await _repository.GetByProviderAsync(request.ProviderName, cancellationToken);
            var encryptedApiKey = !string.IsNullOrWhiteSpace(request.ApiKey)
                ? AiSecretProtector.Protect(request.ApiKey)
                : existing?.EncryptedApiKey ?? string.Empty;

            if (request.IsActive && string.IsNullOrWhiteSpace(encryptedApiKey))
            {
                return ServiceResult<AiProviderSettingDto>.Failure(
                    ErrorMessages.ValidationFailed,
                    "API key is required before activating a provider.");
            }

            var setting = new AiProviderSetting
            {
                ProviderName = request.ProviderName,
                ModelName = request.ModelName.Trim(),
                EncryptedApiKey = encryptedApiKey,
                EndpointUrl = NormalizeOptional(request.EndpointUrl),
                Temperature = request.Temperature,
                MaxOutputTokens = request.MaxOutputTokens,
                TimeoutSeconds = request.TimeoutSeconds,
                IsActive = request.IsActive
            };

            var saved = await _repository.SaveAsync(setting, cancellationToken);
            await _userActivityService.RecordActivityAsync(
                currentUser,
                "AiSettingSaved",
                "AiProviderSetting",
                saved.ProviderName.ToString(),
                $"Saved AI provider setting for {saved.ProviderName}.",
                cancellationToken: cancellationToken);

            return ServiceResult<AiProviderSettingDto>.Success(
                MapToDto(saved),
                "AI provider setting saved.");
        }
        catch (Exception ex)
        {
            return ServiceResult<AiProviderSettingDto>.Failure(ErrorMessages.SystemError, ex.Message);
        }
    }

    public async Task<ServiceResult<AiProviderTestResultDto>> TestSettingAsync(
        AiProviderName providerName,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureAdmin<AiProviderTestResultDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var setting = await _repository.GetByProviderAsync(providerName, cancellationToken);
            if (setting is null || string.IsNullOrWhiteSpace(setting.EncryptedApiKey))
            {
                return ServiceResult<AiProviderTestResultDto>.Failure(
                    ErrorMessages.ValidationFailed,
                    "Save this provider with an API key before testing.");
            }

            var options = CreateRequestOptions(setting);
            var testedAt = DateTime.Now;

            try
            {
                var client = _clientFactory.Create(setting.ProviderName);
                var content = await client.GenerateJsonAsync(
                    options,
                    "Return JSON only.",
                    "Return {\"ok\": true, \"message\": \"connected\"}.",
                    cancellationToken);

                var connected = content.Contains("ok", StringComparison.OrdinalIgnoreCase);
                var status = connected ? "Connected" : "Connected, but response format was unexpected.";
                await _repository.UpdateTestStatusAsync(setting.ProviderName, testedAt, status, cancellationToken);
                await _userActivityService.RecordActivityAsync(
                    currentUser,
                    "AiSettingTested",
                    "AiProviderSetting",
                    setting.ProviderName.ToString(),
                    $"Tested AI provider {setting.ProviderName}: {status}.",
                    cancellationToken: cancellationToken);

                return ServiceResult<AiProviderTestResultDto>.Success(new AiProviderTestResultDto
                {
                    IsConnected = connected,
                    ProviderName = setting.ProviderName.ToString(),
                    ModelName = options.ModelName,
                    Message = status,
                    TestedAt = testedAt
                }, status);
            }
            catch (Exception ex)
            {
                var status = $"Failed: {ex.Message}";
                await _repository.UpdateTestStatusAsync(setting.ProviderName, testedAt, status, cancellationToken);
                await _userActivityService.RecordActivityAsync(
                    currentUser,
                    "AiSettingTested",
                    "AiProviderSetting",
                    setting.ProviderName.ToString(),
                    $"Tested AI provider {setting.ProviderName}: failed.",
                    result: "Failed",
                    errorMessage: ex.Message,
                    cancellationToken: cancellationToken);

                return ServiceResult<AiProviderTestResultDto>.Success(new AiProviderTestResultDto
                {
                    IsConnected = false,
                    ProviderName = setting.ProviderName.ToString(),
                    ModelName = options.ModelName,
                    Message = status,
                    TestedAt = testedAt
                }, status);
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<AiProviderTestResultDto>.Failure(ErrorMessages.SystemError, ex.Message);
        }
    }

    private static AiProviderRequestOptions CreateRequestOptions(AiProviderSetting setting)
    {
        var modelName = setting.ProviderName == AiProviderName.Gemini
            ? AiProviderDefaults.NormalizeGeminiModelName(setting.ModelName)
            : setting.ModelName;
        var endpointUrl = setting.ProviderName == AiProviderName.Gemini
            ? AiProviderDefaults.NormalizeGeminiEndpointUrl(setting.EndpointUrl)
            : setting.EndpointUrl;

        return new AiProviderRequestOptions(
            setting.ProviderName,
            modelName,
            AiSecretProtector.Unprotect(setting.EncryptedApiKey),
            endpointUrl,
            setting.Temperature,
            setting.MaxOutputTokens,
            setting.TimeoutSeconds);
    }

    private static List<string> ValidateRequest(SaveAiProviderSettingRequestDto request)
    {
        var errors = new List<string>();

        if (!Enum.IsDefined(request.ProviderName))
        {
            errors.Add("AI provider is not valid.");
        }

        if (string.IsNullOrWhiteSpace(request.ModelName))
        {
            errors.Add("Model name is required.");
        }
        else if (request.ModelName.Trim().Length > 100)
        {
            errors.Add("Model name cannot exceed 100 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.EndpointUrl)
            && !Uri.TryCreate(request.EndpointUrl.Trim(), UriKind.Absolute, out _))
        {
            errors.Add("Endpoint URL must be absolute.");
        }

        if (request.Temperature is < 0 or > 2)
        {
            errors.Add("Temperature must be between 0 and 2.");
        }

        if (request.MaxOutputTokens is < 100 or > 4000)
        {
            errors.Add("Max output tokens must be between 100 and 4000.");
        }

        if (request.TimeoutSeconds is < 5 or > 180)
        {
            errors.Add("Timeout seconds must be between 5 and 180.");
        }

        return errors;
    }

    private static AiProviderSettingDto MapToDto(AiProviderSetting setting)
    {
        var modelName = setting.ProviderName == AiProviderName.Gemini
            ? AiProviderDefaults.NormalizeGeminiModelName(setting.ModelName)
            : setting.ModelName;
        var endpointUrl = setting.ProviderName == AiProviderName.Gemini
            ? AiProviderDefaults.NormalizeGeminiEndpointUrl(setting.EndpointUrl)
            : setting.EndpointUrl;

        return new AiProviderSettingDto
        {
            AiProviderSettingId = setting.AiProviderSettingId,
            ProviderName = setting.ProviderName,
            ModelName = modelName,
            HasApiKey = !string.IsNullOrWhiteSpace(setting.EncryptedApiKey),
            EndpointUrl = endpointUrl,
            Temperature = setting.Temperature,
            MaxOutputTokens = setting.MaxOutputTokens,
            TimeoutSeconds = setting.TimeoutSeconds,
            IsActive = setting.IsActive,
            LastTestedAt = setting.LastTestedAt,
            LastTestStatus = setting.LastTestStatus ?? string.Empty
        };
    }

    private static AiProviderSettingDto CreateDefaultDto(AiProviderName providerName)
    {
        return new AiProviderSettingDto
        {
            ProviderName = providerName,
            ModelName = providerName == AiProviderName.OpenAI ? "gpt-4o-mini" : AiProviderDefaults.GeminiModelName,
            HasApiKey = false,
            EndpointUrl = providerName == AiProviderName.OpenAI
                ? "https://api.openai.com/v1/chat/completions"
                : AiProviderDefaults.GeminiBaseEndpointUrl,
            Temperature = DefaultTemperature,
            MaxOutputTokens = DefaultMaxOutputTokens,
            TimeoutSeconds = DefaultTimeoutSeconds,
            IsActive = false,
            LastTestStatus = "Not tested"
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static ServiceResult<T>? EnsureAdmin<T>(CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        return currentUser.RoleName == RoleName.Admin
            ? null
            : ServiceResult<T>.Failure(ErrorMessages.Forbidden);
    }
}
