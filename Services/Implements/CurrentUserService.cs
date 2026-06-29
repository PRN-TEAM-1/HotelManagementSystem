using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Services.Interfaces;

namespace Services.Implements;

public sealed class CurrentUserService : ICurrentUserService
{
    private CurrentSessionDto? _currentUser;

    public event EventHandler? SessionChanged;

    public CurrentSessionDto? User => _currentUser;

    public bool IsAuthenticated => _currentUser?.IsAuthenticated == true;

    public string DisplayName => IsAuthenticated ? _currentUser!.FullName : "No active session";

    public string RoleDisplay => IsAuthenticated ? _currentUser!.RoleName.ToString() : "Public";

    public void Set(CurrentSessionDto session)
    {
        _currentUser = session ?? throw new ArgumentNullException(nameof(session));
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        if (_currentUser is null)
        {
            return;
        }

        _currentUser = null;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasRole(params RoleName[] roles)
    {
        return IsAuthenticated && roles.Contains(_currentUser!.RoleName);
    }
}
