namespace BusinessObjects.DTOs;

public sealed class ClientEnvironmentDto
{
    public string MachineName { get; init; } = string.Empty;

    public string WindowsUser { get; init; } = string.Empty;

    public string IpAddress { get; init; } = string.Empty;

    public string OsVersion { get; init; } = string.Empty;

    public string AppVersion { get; init; } = string.Empty;

    public string DeviceType { get; init; } = string.Empty;
}
