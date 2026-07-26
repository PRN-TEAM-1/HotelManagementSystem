using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Services.Interfaces;

namespace WPF.ViewModels;

public sealed class SessionViewModel : BaseViewModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserActivityService _userActivityService;
    private List<LoginSessionDto> _loginSessions = new();
    private List<ActivityLogDto> _activityLogs = new();
    private string _sessionAuditMessage = string.Empty;
    private bool _isBusy;

    public SessionViewModel(
        ICurrentUserService currentUserService,
        IUserActivityService userActivityService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _userActivityService = userActivityService ?? throw new ArgumentNullException(nameof(userActivityService));
        _currentUserService.SessionChanged += OnSessionChanged;

        RefreshCommand = new WPF.Commands.AsyncRelayCommand(LoadSessionAuditAsync, () => !IsBusy);
        RefreshSessionSnapshot();
    }

    public override string Title => "Session";

    public override string Description => "Account and access summary";

    public ObservableCollection<string> AccessibleAreas { get; } = new();

    public WPF.Commands.AsyncRelayCommand RefreshCommand { get; }

    public List<LoginSessionDto> LoginSessions
    {
        get => _loginSessions;
        private set => SetProperty(ref _loginSessions, value);
    }

    public List<ActivityLogDto> ActivityLogs
    {
        get => _activityLogs;
        private set => SetProperty(ref _activityLogs, value);
    }

    public string SessionState => _currentUserService.IsAuthenticated ? "Authenticated" : "Signed out";

    public string DisplayName => _currentUserService.User?.FullName ?? "No active session";

    public string Username => _currentUserService.User?.Username ?? "guest";

    public string RoleDisplay => _currentUserService.User?.RoleName.ToString() ?? "Public";

    public string Email => _currentUserService.User?.Email ?? "Not available";

    public string LoggedInAtDisplay =>
        _currentUserService.User is null
            ? "Waiting for login"
            : _currentUserService.User.LoggedInAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    public string LoginSessionIdDisplay => _currentUserService.User?.LoginSessionId?.ToString() ?? "Not tracked";

    public string MachineName => _currentUserService.User?.MachineName ?? "Unknown";

    public string WindowsUser => _currentUserService.User?.WindowsUser ?? "Unknown";

    public string IpAddress => _currentUserService.User?.IpAddress ?? "Unknown";

    public string OsVersion => _currentUserService.User?.OsVersion ?? "Unknown";

    public string AppVersion => _currentUserService.User?.AppVersion ?? "Unknown";

    public string DeviceType => _currentUserService.User?.DeviceType ?? "Windows Desktop";

    public string SessionAuditMessage
    {
        get => _sessionAuditMessage;
        private set => SetProperty(ref _sessionAuditMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public override void OnNavigatedTo()
    {
        _ = LoadSessionAuditAsync();
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshSessionSnapshot();
        _ = LoadSessionAuditAsync();
    }

    private void RefreshSessionSnapshot()
    {
        AccessibleAreas.Clear();

        foreach (var area in GetAreasForCurrentRole())
        {
            AccessibleAreas.Add(area);
        }

        OnPropertiesChanged(
            nameof(SessionState),
            nameof(DisplayName),
            nameof(Username),
            nameof(RoleDisplay),
            nameof(Email),
            nameof(LoggedInAtDisplay),
            nameof(LoginSessionIdDisplay),
            nameof(MachineName),
            nameof(WindowsUser),
            nameof(IpAddress),
            nameof(OsVersion),
            nameof(AppVersion),
            nameof(DeviceType));
    }

    private async Task LoadSessionAuditAsync()
    {
        if (_currentUserService.User is null)
        {
            LoginSessions = new();
            ActivityLogs = new();
            return;
        }

        IsBusy = true;
        SessionAuditMessage = string.Empty;

        try
        {
            var currentUser = _currentUserService.User;
            var sessionsResult = await _userActivityService.GetUserLoginSessionsAsync(
                currentUser.UserId,
                currentUser);
            var logsResult = await _userActivityService.GetUserActivityLogsAsync(
                currentUser.UserId,
                currentUser);

            LoginSessions = sessionsResult.IsSuccess ? sessionsResult.Data ?? new() : new();
            ActivityLogs = logsResult.IsSuccess ? logsResult.Data ?? new() : new();

            if (sessionsResult.IsFailure)
            {
                SessionAuditMessage = sessionsResult.Errors.FirstOrDefault() ?? sessionsResult.Message;
            }
            else if (logsResult.IsFailure)
            {
                SessionAuditMessage = logsResult.Errors.FirstOrDefault() ?? logsResult.Message;
            }
        }
        catch (Exception ex)
        {
            SessionAuditMessage = $"Unable to load session history: {ex.Message}";
            LoginSessions = new();
            ActivityLogs = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private IEnumerable<string> GetAreasForCurrentRole()
    {
        yield return "Workspace";
        yield return "Session";

        if (_currentUserService.HasRole(RoleName.Admin))
        {
            yield return "Account Management";
            yield return "Admin Setup";
            yield return "AI Settings";
            yield return "Reports";
            yield break;
        }

        if (_currentUserService.HasRole(RoleName.Manager))
        {
            yield return "Reports";
            yield break;
        }

        if (_currentUserService.HasRole(RoleName.Receptionist))
        {
            yield return "Operations";
        }
    }
}
