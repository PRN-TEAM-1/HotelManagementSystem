using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class AiProviderSetting
{
    public int AiProviderSettingId { get; set; }

    public AiProviderName ProviderName { get; set; }

    public string ModelName { get; set; } = string.Empty;

    public string EncryptedApiKey { get; set; } = string.Empty;

    public string? EndpointUrl { get; set; }

    public decimal Temperature { get; set; }

    public int MaxOutputTokens { get; set; }

    public int TimeoutSeconds { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastTestedAt { get; set; }

    public string? LastTestStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
