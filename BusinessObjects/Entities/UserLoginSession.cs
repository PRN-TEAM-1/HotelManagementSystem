namespace BusinessObjects.Entities;

public sealed class UserLoginSession
{
    public int LoginSessionId { get; set; }

    public int UserId { get; set; }

    public DateTime LoginAtUtc { get; set; }

    public DateTime? LogoutAtUtc { get; set; }

    public DateTime LastSeenAtUtc { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public string WindowsUser { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string OsVersion { get; set; } = string.Empty;

    public string AppVersion { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public User? User { get; set; }

    public ICollection<UserActivityLog> ActivityLogs { get; set; } = new List<UserActivityLog>();
}
