using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using WPF.Commands;
using WPF.Helpers;

namespace WPF.ViewModels;

public sealed class SessionViewModel : BaseViewModel
{
    private readonly CurrentSession _currentSession;
    private readonly DialogService _dialogService;

    public SessionViewModel(CurrentSession currentSession, DialogService dialogService)
    {
        _currentSession = currentSession ?? throw new ArgumentNullException(nameof(currentSession));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        UseAdminSessionCommand = new RelayCommand(() => ApplyDemoSession(RoleName.Admin));
        UseManagerSessionCommand = new RelayCommand(() => ApplyDemoSession(RoleName.Manager));
        UseReceptionistSessionCommand = new RelayCommand(() => ApplyDemoSession(RoleName.Receptionist));
        ClearSessionCommand = new RelayCommand(ClearSession, () => _currentSession.IsAuthenticated);
        ShowSessionSummaryCommand = new RelayCommand(ShowSessionSummary);

        _currentSession.SessionChanged += OnSessionChanged;

        RefreshSessionSnapshot();
    }

    public override string Title => "Session Sandbox";

    public override string Description =>
        "Switch between demo staff roles to validate CurrentSession, role-aware navigation and shared dialog flows before the real login feature lands.";

    public RelayCommand UseAdminSessionCommand { get; }

    public RelayCommand UseManagerSessionCommand { get; }

    public RelayCommand UseReceptionistSessionCommand { get; }

    public RelayCommand ClearSessionCommand { get; }

    public RelayCommand ShowSessionSummaryCommand { get; }

    public ObservableCollection<string> AccessibleAreas { get; } = new();

    public string SessionState => _currentSession.IsAuthenticated ? "Authenticated demo session" : "Signed out";

    public string DisplayName => _currentSession.User?.FullName ?? "No active session";

    public string Username => _currentSession.User?.Username ?? "guest";

    public string RoleDisplay => _currentSession.User?.RoleName.ToString() ?? "Public";

    public string LoggedInAtDisplay =>
        _currentSession.User is null
            ? "Waiting for login"
            : _currentSession.User.LoggedInAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    private void ApplyDemoSession(RoleName roleName)
    {
        var demoSession = roleName switch
        {
            RoleName.Admin => new CurrentSessionDto
            {
                UserId = 1,
                RoleId = (int)RoleName.Admin,
                Username = "admin.core",
                FullName = "Core Demo Admin",
                Email = "admin@hotel.local",
                RoleName = RoleName.Admin,
                LoggedInAtUtc = DateTime.UtcNow
            },
            RoleName.Manager => new CurrentSessionDto
            {
                UserId = 2,
                RoleId = (int)RoleName.Manager,
                Username = "manager.insight",
                FullName = "Core Demo Manager",
                Email = "manager@hotel.local",
                RoleName = RoleName.Manager,
                LoggedInAtUtc = DateTime.UtcNow
            },
            _ => new CurrentSessionDto
            {
                UserId = 3,
                RoleId = (int)RoleName.Receptionist,
                Username = "reception.ops",
                FullName = "Core Demo Receptionist",
                Email = "reception@hotel.local",
                RoleName = RoleName.Receptionist,
                LoggedInAtUtc = DateTime.UtcNow
            }
        };

        _currentSession.Set(demoSession);
    }

    private void ClearSession()
    {
        if (!_currentSession.IsAuthenticated)
        {
            return;
        }

        if (_dialogService.Confirm("Clear the current demo session and return to foundation-only navigation?", "Clear Demo Session"))
        {
            _currentSession.Clear();
        }
    }

    private void ShowSessionSummary()
    {
        var summary = _currentSession.IsAuthenticated
            ? $"User: {DisplayName}\nRole: {RoleDisplay}\nUsername: {Username}\nAccess: {string.Join(", ", AccessibleAreas)}"
            : "No demo session is active. Use one of the role buttons to validate role-aware navigation.";

        _dialogService.ShowInfo(summary, "Current Session");
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshSessionSnapshot();
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
            nameof(LoggedInAtDisplay));

        ClearSessionCommand.RaiseCanExecuteChanged();
    }

    private IEnumerable<string> GetAreasForCurrentRole()
    {
        yield return "Core Workspace";
        yield return "Session Sandbox";
        yield return "Style Guide";

        if (_currentSession.HasRole(RoleName.Admin))
        {
            yield return "Administration";
            yield return "Operations";
            yield return "Reports";
            yield break;
        }

        if (_currentSession.HasRole(RoleName.Manager))
        {
            yield return "Reports";
            yield break;
        }

        if (_currentSession.HasRole(RoleName.Receptionist))
        {
            yield return "Operations";
        }
    }
}
