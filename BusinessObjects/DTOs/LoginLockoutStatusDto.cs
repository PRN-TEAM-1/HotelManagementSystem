namespace BusinessObjects.DTOs;

public sealed class LoginLockoutStatusDto
{
    public int FailedAttemptCount { get; set; }

    public DateTime? LatestFailedAttemptAtUtc { get; set; }
}
