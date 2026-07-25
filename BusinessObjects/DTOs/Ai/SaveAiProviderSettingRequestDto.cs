using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public sealed class SaveAiProviderSettingRequestDto
{
    public AiProviderName ProviderName { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string? ApiKey { get; set; }

    public string? EndpointUrl { get; set; }

    public decimal Temperature { get; set; }

    public int MaxOutputTokens { get; set; }

    public int TimeoutSeconds { get; set; }

    public bool IsActive { get; set; }
}
