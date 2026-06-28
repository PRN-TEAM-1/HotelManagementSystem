using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public sealed class CurrentSessionDto
{
    public int UserId { get; init; }

    public int RoleId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public RoleName RoleName { get; init; }

    public DateTime LoggedInAtUtc { get; init; } = DateTime.UtcNow;

    public bool IsAuthenticated => UserId > 0;
}
