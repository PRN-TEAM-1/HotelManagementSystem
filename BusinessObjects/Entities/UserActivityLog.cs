namespace BusinessObjects.Entities;

public sealed class UserActivityLog
{
    public int ActivityLogId { get; set; }

    public int? LoginSessionId { get; set; }

    public int? ActorUserId { get; set; }

    public int? TargetUserId { get; set; }

    public string? AttemptedUsername { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? OldValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public string Result { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;

    public UserLoginSession? LoginSession { get; set; }

    public User? ActorUser { get; set; }

    public User? TargetUser { get; set; }
}
