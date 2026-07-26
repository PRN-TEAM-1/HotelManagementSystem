using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public sealed class CurrentSessionDto
{
    public int UserId { get; init; }

    public int RoleId { get; init; }

    public int? LoginSessionId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public RoleName RoleName { get; init; }

    public DateTime LoggedInAtUtc { get; init; } = DateTime.UtcNow;

    public long UserUpdatedAtTicks { get; init; }

    public string MachineName { get; init; } = string.Empty;

    public string WindowsUser { get; init; } = string.Empty;

    public string IpAddress { get; init; } = string.Empty;

    public string OsVersion { get; init; } = string.Empty;

    public string AppVersion { get; init; } = string.Empty;

    public string DeviceType { get; init; } = string.Empty;

    public bool IsAuthenticated => UserId > 0;
}
