using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;
using WPF.Helpers;

namespace WPF.ViewModels;

public sealed class LoginViewModel : BaseViewModel
{
    private static readonly TimeSpan RememberedLoginLifetime = TimeSpan.FromDays(3);

    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly RememberedLoginStore _rememberedLoginStore;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;
    private bool _isPasswordRevealed;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(
        IAuthService authService,
        ICurrentUserService currentUserService,
        RememberedLoginStore? rememberedLoginStore = null,
        string? initialErrorMessage = null)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _rememberedLoginStore = rememberedLoginStore ?? new RememberedLoginStore();
        _errorMessage = initialErrorMessage?.Trim() ?? string.Empty;

        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ErrorMessage = string.Empty;
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ErrorMessage = string.Empty;
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public bool IsPasswordRevealed
    {
        get => _isPasswordRevealed;
        set => SetProperty(ref _isPasswordRevealed, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public AsyncRelayCommand LoginCommand { get; }

    private bool CanLogin()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrWhiteSpace(Password);
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new LoginRequestDto
            {
                Username = Username.Trim(),
                Password = Password,
                ClientEnvironment = ClientEnvironmentProvider.Capture()
            };

            var result = await _authService.LoginAsync(request);

            if (result.IsFailure || result.Data?.CurrentSession is null)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            if (RememberMe)
            {
                _rememberedLoginStore.Save(result.Data.CurrentSession, RememberedLoginLifetime);
            }
            else
            {
                _rememberedLoginStore.Clear();
            }

            _currentUserService.Set(result.Data.CurrentSession);
        }
        catch
        {
            ErrorMessage = BusinessObjects.Constants.ErrorMessages.DatabaseConnectionRequired;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
