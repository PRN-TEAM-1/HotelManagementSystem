using System.Globalization;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class AiSettingsViewModel : BaseViewModel
{
    private readonly IAiConfigurationService _aiConfigurationService;
    private readonly ICurrentUserService _currentUserService;

    private List<AiProviderSettingDto> _settings = new();
    private LookupItemDto? _selectedProviderOption;
    private string _modelName = string.Empty;
    private string _apiKeyText = string.Empty;
    private string _endpointUrl = string.Empty;
    private string _temperatureText = "0.2";
    private string _maxOutputTokensText = "900";
    private string _timeoutSecondsText = "45";
    private string _selectedProviderStatus = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _hasSavedApiKey;
    private bool _isActive;
    private bool _isBusy;
    private bool _isInitialized;

    public AiSettingsViewModel(
        IAiConfigurationService aiConfigurationService,
        ICurrentUserService currentUserService)
    {
        _aiConfigurationService = aiConfigurationService ?? throw new ArgumentNullException(nameof(aiConfigurationService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        ProviderOptions = Enum.GetValues<AiProviderName>()
            .Select(provider => new LookupItemDto
            {
                Id = (int)provider,
                Value = provider.ToString(),
                DisplayName = provider.ToString()
            })
            .ToList();

        LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync, CanExecuteWhenReady);
        SaveSettingCommand = new AsyncRelayCommand(SaveSettingAsync, CanExecuteWhenReady);
        TestSettingCommand = new AsyncRelayCommand(TestSettingAsync, CanTestSetting);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
    }

    public override string Title => "AI Settings";

    public override string Description => "Configure OpenAI or Gemini provider for assistant features";

    public List<LookupItemDto> ProviderOptions { get; }

    public List<AiProviderSettingDto> Settings
    {
        get => _settings;
        private set => SetProperty(ref _settings, value);
    }

    public LookupItemDto? SelectedProviderOption
    {
        get => _selectedProviderOption;
        set
        {
            if (SetProperty(ref _selectedProviderOption, value))
            {
                ApplySelectedProvider();
                SaveSettingCommand.RaiseCanExecuteChanged();
                TestSettingCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ModelName
    {
        get => _modelName;
        set => SetProperty(ref _modelName, value);
    }

    public string ApiKeyText
    {
        get => _apiKeyText;
        set
        {
            if (SetProperty(ref _apiKeyText, value))
            {
                TestSettingCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string EndpointUrl
    {
        get => _endpointUrl;
        set => SetProperty(ref _endpointUrl, value);
    }

    public string TemperatureText
    {
        get => _temperatureText;
        set => SetProperty(ref _temperatureText, value);
    }

    public string MaxOutputTokensText
    {
        get => _maxOutputTokensText;
        set => SetProperty(ref _maxOutputTokensText, value);
    }

    public string TimeoutSecondsText
    {
        get => _timeoutSecondsText;
        set => SetProperty(ref _timeoutSecondsText, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool HasSavedApiKey
    {
        get => _hasSavedApiKey;
        private set => SetProperty(ref _hasSavedApiKey, value);
    }

    public string SelectedProviderStatus
    {
        get => _selectedProviderStatus;
        private set => SetProperty(ref _selectedProviderStatus, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                LoadSettingsCommand.RaiseCanExecuteChanged();
                SaveSettingCommand.RaiseCanExecuteChanged();
                TestSettingCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public AsyncRelayCommand LoadSettingsCommand { get; }

    public AsyncRelayCommand SaveSettingCommand { get; }

    public AsyncRelayCommand TestSettingCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public override async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await LoadSettingsAsync();
        _isInitialized = true;
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        if (!_isInitialized && !IsBusy)
        {
            _ = LoadSettingsAsync();
        }
    }

    private bool CanExecuteWhenReady()
    {
        return !IsBusy;
    }

    private bool CanTestSetting()
    {
        return !IsBusy
            && SelectedProviderOption is not null
            && (HasSavedApiKey || !string.IsNullOrWhiteSpace(ApiKeyText));
    }

    private async Task LoadSettingsAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _aiConfigurationService.GetSettingsAsync(_currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                Settings = new();
                return;
            }

            Settings = result.Data ?? new();
            var selectedProvider = SelectedProviderOption?.Value
                ?? Settings.FirstOrDefault(setting => setting.IsActive)?.ProviderDisplayName
                ?? Settings.FirstOrDefault()?.ProviderDisplayName;

            SelectedProviderOption = ProviderOptions.FirstOrDefault(option =>
                string.Equals(option.Value, selectedProvider, StringComparison.OrdinalIgnoreCase));

            ApplySelectedProvider();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading AI settings: {ex.Message}";
            Settings = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveSettingAsync()
    {
        if (!TryBuildSaveRequest(out var request, out var validationError))
        {
            ErrorMessage = validationError;
            SuccessMessage = string.Empty;
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _aiConfigurationService.SaveSettingAsync(request, _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = result.Message;
            ApiKeyText = string.Empty;
            await LoadSettingsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving AI setting: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestSettingAsync()
    {
        if (SelectedProviderOption is null)
        {
            ErrorMessage = "Select an AI provider first.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(ApiKeyText))
        {
            await SaveSettingAsync();
            if (!string.IsNullOrWhiteSpace(ErrorMessage))
            {
                return;
            }
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var provider = (AiProviderName)SelectedProviderOption.Id.GetValueOrDefault();
            var result = await _aiConfigurationService.TestSettingAsync(provider, _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            var testResult = result.Data;
            if (testResult?.IsConnected == true)
            {
                SuccessMessage = testResult.Message;
            }
            else
            {
                ErrorMessage = testResult?.Message ?? result.Message;
            }

            await LoadSettingsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error testing AI provider: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool TryBuildSaveRequest(
        out SaveAiProviderSettingRequestDto request,
        out string validationError)
    {
        request = new SaveAiProviderSettingRequestDto();
        validationError = string.Empty;

        if (SelectedProviderOption is null)
        {
            validationError = "Select an AI provider first.";
            return false;
        }

        if (!TryParseDecimal(TemperatureText, out var temperature))
        {
            validationError = "Temperature must be a valid number.";
            return false;
        }

        if (!int.TryParse(MaxOutputTokensText, out var maxTokens))
        {
            validationError = "Max output tokens must be a valid integer.";
            return false;
        }

        if (!int.TryParse(TimeoutSecondsText, out var timeoutSeconds))
        {
            validationError = "Timeout seconds must be a valid integer.";
            return false;
        }

        request = new SaveAiProviderSettingRequestDto
        {
            ProviderName = (AiProviderName)SelectedProviderOption.Id.GetValueOrDefault(),
            ModelName = ModelName,
            ApiKey = ApiKeyText,
            EndpointUrl = EndpointUrl,
            Temperature = temperature,
            MaxOutputTokens = maxTokens,
            TimeoutSeconds = timeoutSeconds,
            IsActive = IsActive
        };

        return true;
    }

    private void ApplySelectedProvider()
    {
        if (SelectedProviderOption is null)
        {
            return;
        }

        var setting = Settings.FirstOrDefault(item =>
            string.Equals(item.ProviderDisplayName, SelectedProviderOption.Value, StringComparison.OrdinalIgnoreCase));

        if (setting is null)
        {
            return;
        }

        ModelName = setting.ModelName;
        ApiKeyText = string.Empty;
        EndpointUrl = setting.EndpointUrl ?? string.Empty;
        TemperatureText = setting.Temperature.ToString("0.##", CultureInfo.InvariantCulture);
        MaxOutputTokensText = setting.MaxOutputTokens.ToString(CultureInfo.InvariantCulture);
        TimeoutSecondsText = setting.TimeoutSeconds.ToString(CultureInfo.InvariantCulture);
        IsActive = setting.IsActive;
        HasSavedApiKey = setting.HasApiKey;

        var testStatus = string.IsNullOrWhiteSpace(setting.LastTestStatus)
            ? "Not tested"
            : setting.LastTestStatus;
        SelectedProviderStatus = setting.IsActive
            ? $"Active provider. {testStatus}"
            : testStatus;
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        value = (value ?? string.Empty).Trim().Replace(',', '.');
        return decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out result);
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
