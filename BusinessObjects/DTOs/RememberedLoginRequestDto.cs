namespace BusinessObjects.DTOs;

public sealed class RememberedLoginRequestDto
{
    public int UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public long UserUpdatedAtTicks { get; init; }

    public DateTime ExpiresAtUtc { get; init; }

    public ClientEnvironmentDto ClientEnvironment { get; init; } = new();
}
