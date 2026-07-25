namespace BusinessObjects.DTOs;

public sealed class AiProviderTestResultDto
{
    public bool IsConnected { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime TestedAt { get; set; }
}
