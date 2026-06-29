using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

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
                Password = Password
            };

            var result = await _authService.LoginAsync(request);

            if (result.IsFailure || result.Data?.CurrentSession is null)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            _currentUserService.Set(result.Data.CurrentSession);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
