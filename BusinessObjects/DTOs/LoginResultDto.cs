namespace BusinessObjects.DTOs;

public sealed class LoginResultDto
{
    public CurrentSessionDto CurrentSession { get; init; } = new();

    public string WelcomeMessage { get; init; } = string.Empty;
}
