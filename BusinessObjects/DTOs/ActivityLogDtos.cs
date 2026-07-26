namespace BusinessObjects.DTOs;

public sealed class LoginSessionDto
{
    public int LoginSessionId { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DateTime LoginAtUtc { get; set; }

    public DateTime LoginAtLocal => ToLocal(LoginAtUtc);

    public DateTime? LogoutAtUtc { get; set; }

    public DateTime? LogoutAtLocal => LogoutAtUtc.HasValue ? ToLocal(LogoutAtUtc.Value) : null;

    public DateTime LastSeenAtUtc { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public string WindowsUser { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string OsVersion { get; set; } = string.Empty;

    public string AppVersion { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    private static DateTime ToLocal(DateTime utc)
    {
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime();
    }
}

public sealed class ActivityLogDto
{
    public int ActivityLogId { get; set; }

    public int? LoginSessionId { get; set; }

    public int? ActorUserId { get; set; }

    public string ActorUsername { get; set; } = string.Empty;

    public string ActorFullName { get; set; } = string.Empty;

    public int? TargetUserId { get; set; }

    public string TargetUsername { get; set; } = string.Empty;

    public string? AttemptedUsername { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Result { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public DateTime OccurredAtLocal => DateTime.SpecifyKind(OccurredAtUtc, DateTimeKind.Utc).ToLocalTime();

    public string MachineName { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;

    public string DeviceType { get; set; } = string.Empty;
}

public sealed class ActivityLogRequestDto
{
    public int? LoginSessionId { get; init; }

    public int? ActorUserId { get; init; }

    public int? TargetUserId { get; init; }

    public string? AttemptedUsername { get; init; }

    public string ActionType { get; init; } = string.Empty;

    public string EntityName { get; init; } = string.Empty;

    public string? EntityId { get; init; }

    public string Description { get; init; } = string.Empty;

    public string? OldValuesJson { get; init; }

    public string? NewValuesJson { get; init; }

    public string Result { get; init; } = "Success";

    public string? ErrorMessage { get; init; }

    public ClientEnvironmentDto? Environment { get; init; }
}

public sealed class ActivityDashboardDto
{
    public int TodayLoginCount { get; set; }

    public int TodayFailedLoginCount { get; set; }

    public int TodayLogoutCount { get; set; }

    public int TodayActivityCount { get; set; }

    public int MaxDailyActivityCount { get; set; } = 1;

    public List<ActivityTrendDto> DailyActivity { get; set; } = new();

    public List<TopActiveUserDto> TopActiveUsers { get; set; } = new();

    public List<ActivityLogDto> RecentActivities { get; set; } = new();
}

public sealed class ActivityTrendDto
{
    public DateTime Date { get; set; }

    public string DateLabel => Date.ToString("MM-dd");

    public int ActivityCount { get; set; }
}

public sealed class TopActiveUserDto
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public int ActivityCount { get; set; }
}
