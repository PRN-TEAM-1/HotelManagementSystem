using BusinessObjects.DTOs;
using BusinessObjects.Enums;

namespace Services.Interfaces;

public interface ICurrentUserService
{
    event EventHandler? SessionChanged;

    CurrentSessionDto? User { get; }

    bool IsAuthenticated { get; }

    string DisplayName { get; }

    string RoleDisplay { get; }

    void Set(CurrentSessionDto session);

    void Clear();

    bool HasRole(params RoleName[] roles);
}
