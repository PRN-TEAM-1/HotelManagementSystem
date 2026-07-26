namespace BusinessObjects.DTOs;

public sealed class LoginRequestDto
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public ClientEnvironmentDto ClientEnvironment { get; init; } = new();
}
