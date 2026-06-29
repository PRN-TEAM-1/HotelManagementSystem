using System.Collections.ObjectModel;
using BusinessObjects.Enums;
using Services.Interfaces;

namespace WPF.ViewModels;

public sealed class SessionViewModel : BaseViewModel
{
    private readonly ICurrentUserService _currentUserService;

    public SessionViewModel(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _currentUserService.SessionChanged += OnSessionChanged;

        RefreshSessionSnapshot();
    }

    public override string Title => "Current Session";

    public override string Description =>
        "Read-only session summary for the authenticated staff account resolved by the real login flow.";

    public ObservableCollection<string> AccessibleAreas { get; } = new();

    public string SessionState => _currentUserService.IsAuthenticated ? "Authenticated" : "Signed out";

    public string DisplayName => _currentUserService.User?.FullName ?? "No active session";

    public string Username => _currentUserService.User?.Username ?? "guest";

    public string RoleDisplay => _currentUserService.User?.RoleName.ToString() ?? "Public";

    public string Email => _currentUserService.User?.Email ?? "Not available";

    public string LoggedInAtDisplay =>
        _currentUserService.User is null
            ? "Waiting for login"
            : _currentUserService.User.LoggedInAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

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
            nameof(Email),
            nameof(LoggedInAtDisplay));
    }

    private IEnumerable<string> GetAreasForCurrentRole()
    {
        yield return "Core Workspace";
        yield return "Current Session";
        yield return "Style Guide";

        if (_currentUserService.HasRole(RoleName.Admin))
        {
            yield return "Administration";
            yield return "Operations";
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
