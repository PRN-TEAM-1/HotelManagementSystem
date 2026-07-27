using System.Text.Json;
using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.AI;
using Services.Interfaces;

namespace Services.Implements;

public sealed class AiServiceRecommendationService : IAiServiceRecommendationService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IAiProviderSettingRepository _providerSettingRepository;
    private readonly IAiRecommendationRepository _recommendationRepository;
    private readonly AiProviderClientFactory _clientFactory;

    public AiServiceRecommendationService(
        IAiProviderSettingRepository? providerSettingRepository = null,
        IAiRecommendationRepository? recommendationRepository = null,
        AiProviderClientFactory? clientFactory = null)
    {
        _providerSettingRepository = providerSettingRepository ?? new AiProviderSettingRepository();
        _recommendationRepository = recommendationRepository ?? new AiRecommendationRepository();
        _clientFactory = clientFactory ?? new AiProviderClientFactory();
    }

    public async Task<ServiceResult<AiServiceRecommendationResponseDto>> GetRecommendationsAsync(
        AiServiceRecommendationRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorizationResult = EnsureCanUseAi<AiServiceRecommendationResponseDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        if (request.BookingDetailId <= 0)
        {
            return ServiceResult<AiServiceRecommendationResponseDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var setting = await _providerSettingRepository.GetActiveAsync(cancellationToken);
            if (setting is null || string.IsNullOrWhiteSpace(setting.EncryptedApiKey))
            {
                return ServiceResult<AiServiceRecommendationResponseDto>.Failure(
                    ErrorMessages.BusinessRuleViolation,
                    "AI provider is not configured. Ask Admin to activate OpenAI or Gemini first.");
            }

            var context = await _recommendationRepository.GetServiceRecommendationContextAsync(
                request.BookingDetailId,
                cancellationToken);

            if (context is null)
            {
                return ServiceResult<AiServiceRecommendationResponseDto>.Failure(
                    ErrorMessages.NotFound,
                    "Select a checked-in stay before requesting AI recommendations.");
            }

            if (context.ActiveServices.Count == 0)
            {
                return ServiceResult<AiServiceRecommendationResponseDto>.Failure(
                    ErrorMessages.BusinessRuleViolation,
                    "No active services are available for recommendation.");
            }

            var options = CreateRequestOptions(setting);
            var client = _clientFactory.Create(setting.ProviderName);

            var content = await client.GenerateJsonAsync(
                options,
                BuildSystemPrompt(),
                BuildUserPrompt(context, request),
                cancellationToken);

            var response = ParseRecommendationResponse(
                content,
                setting,
                context,
                request.MaxRecommendations);

            return ServiceResult<AiServiceRecommendationResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<AiServiceRecommendationResponseDto>.Failure(
                "AI recommendation failed.",
                ex.Message);
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

    private static string BuildSystemPrompt()
    {
        return """
            You are an assistant for a hotel front desk. Recommend add-on hotel services that are useful for the current guest stay.
            Return JSON only. The JSON shape must be:
            {
              "summary": "short operational summary for staff",
              "recommendations": [
                {
                  "serviceId": 1,
                  "serviceName": "service name from catalog",
                  "suggestedQuantity": 1,
                  "confidence": 0.85,
                  "reason": "short staff-facing reason",
                  "upsellMessage": "short sentence staff can say to guest"
                }
              ]
            }
            Only recommend serviceId values from activeServices. Do not invent services. Avoid services already in existingServiceOrders.
            Keep each reason under 180 characters. Use Vietnamese for summary, reason, and upsellMessage.
            """;
    }

    private static string BuildUserPrompt(
        AiServiceRecommendationContextDto context,
        AiServiceRecommendationRequestDto request)
    {
        var maxRecommendations = Math.Clamp(request.MaxRecommendations, 1, 5);
        var payload = new
        {
            task = "service_recommendation",
            generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            maxRecommendations,
            guestPreference = string.IsNullOrWhiteSpace(request.GuestPreference)
                ? "No additional preference provided."
                : request.GuestPreference.Trim(),
            rules = new[]
            {
                "Recommend practical services that can be ordered during check-in or active stay.",
                "Prioritize comfort, convenience, and matching guest history.",
                "Do not recommend a service already ordered for this booking detail.",
                "If confidence is low, return fewer recommendations."
            },
            stay = new
            {
                context.BookingDetailId,
                context.BookingId,
                context.CustomerName,
                context.RoomNumber,
                context.RoomType,
                checkInDate = context.CheckInDate.ToString("yyyy-MM-dd"),
                checkOutDate = context.CheckOutDate.ToString("yyyy-MM-dd"),
                actualCheckInDate = context.ActualCheckInDate?.ToString("yyyy-MM-dd HH:mm"),
                context.NumberOfNights,
                context.RoomPrice,
                context.RoomTotal,
                context.GuestStayCount,
                context.CurrentServiceTotal,
                context.BookingNote,
                context.StayNote,
                context.CheckInNote
            },
            activeServices = context.ActiveServices,
            existingServiceOrders = context.ExistingServiceOrders,
            guestServiceHistory = context.GuestServiceHistory
        };

        return JsonSerializer.Serialize(payload, JsonSerializerOptions);
    }

    private static AiServiceRecommendationResponseDto ParseRecommendationResponse(
        string aiContent,
        AiProviderSetting setting,
        AiServiceRecommendationContextDto context,
        int requestedMaxRecommendations)
    {
        var maxRecommendations = Math.Clamp(requestedMaxRecommendations, 1, 5);
        var catalogById = context.ActiveServices.ToDictionary(service => service.ServiceId);
        var catalogByName = context.ActiveServices
            .GroupBy(service => service.ServiceName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var alreadyOrderedServiceIds = context.ExistingServiceOrders
            .Select(order => order.ServiceId)
            .ToHashSet();

        using var document = JsonDocument.Parse(AiJsonResponseHelper.ExtractJsonObject(aiContent));
        var root = document.RootElement;
        var summary = GetString(root, "summary")
            ?? "AI da tao goi y dua tren phong, lich su khach va dich vu hien co.";

        var recommendations = new List<AiRecommendedServiceDto>();
        if (TryGetProperty(root, "recommendations", out var recommendationElement)
            && recommendationElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in recommendationElement.EnumerateArray())
            {
                var serviceId = GetInt(item, "serviceId") ?? GetInt(item, "service_id") ?? 0;
                var serviceName = GetString(item, "serviceName") ?? GetString(item, "service_name") ?? string.Empty;

                AiCatalogServiceDto? catalogService = null;
                if (serviceId > 0)
                {
                    catalogById.TryGetValue(serviceId, out catalogService);
                }

                if (catalogService is null && !string.IsNullOrWhiteSpace(serviceName))
                {
                    catalogByName.TryGetValue(serviceName.Trim(), out catalogService);
                }

                if (catalogService is null || alreadyOrderedServiceIds.Contains(catalogService.ServiceId))
                {
                    continue;
                }

                if (recommendations.Any(row => row.ServiceId == catalogService.ServiceId))
                {
                    continue;
                }

                var suggestedQuantity = Math.Clamp(
                    GetInt(item, "suggestedQuantity") ?? GetInt(item, "suggested_quantity") ?? 1,
                    1,
                    20);

                var confidence = Math.Clamp(
                    GetDecimal(item, "confidence") ?? 0.6m,
                    0m,
                    1m);

                recommendations.Add(new AiRecommendedServiceDto
                {
                    ServiceId = catalogService.ServiceId,
                    ServiceName = catalogService.ServiceName,
                    Category = catalogService.Category,
                    UnitPrice = catalogService.Price,
                    SuggestedQuantity = suggestedQuantity,
                    Confidence = confidence,
                    Reason = Limit(GetString(item, "reason") ?? "Phu hop voi nhu cau hien tai cua khach.", 180),
                    UpsellMessage = Limit(
                        GetString(item, "upsellMessage")
                            ?? GetString(item, "upsell_message")
                            ?? "Anh/chi co muon them dich vu nay cho ky luu tru khong?",
                        180)
                });

                if (recommendations.Count >= maxRecommendations)
                {
                    break;
                }
            }
        }

        return new AiServiceRecommendationResponseDto
        {
            ProviderName = setting.ProviderName.ToString(),
            ModelName = setting.ProviderName == AiProviderName.Gemini
                ? AiProviderDefaults.NormalizeGeminiModelName(setting.ModelName)
                : setting.ModelName,
            Summary = Limit(summary, 260),
            Recommendations = recommendations
        };
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => NormalizeText(value.GetString()),
            JsonValueKind.Number => value.ToString(),
            _ => null
        };
    }

    private static int? GetInt(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number)
            ? number
            : null;
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), out number)
            ? number
            : null;
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string Limit(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = value.Trim();
        return value.Length <= maxLength ? value : $"{value[..maxLength]}...";
    }

    private static ServiceResult<T>? EnsureCanUseAi<T>(CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        if (currentUser.RoleName is not (RoleName.Admin or RoleName.Manager or RoleName.Receptionist))
        {
            return ServiceResult<T>.Failure(ErrorMessages.Forbidden);
        }

        return null;
    }
}
