using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public sealed class AiProviderSettingDto
{
    public int AiProviderSettingId { get; set; }

    public AiProviderName ProviderName { get; set; }

    public string ProviderDisplayName => ProviderName.ToString();

    public string ModelName { get; set; } = string.Empty;

    public bool HasApiKey { get; set; }

    public string ApiKeyStatus => HasApiKey ? "Saved" : "Missing";

    public string? EndpointUrl { get; set; }

    public decimal Temperature { get; set; }

    public int MaxOutputTokens { get; set; }

    public int TimeoutSeconds { get; set; }

    public bool IsActive { get; set; }

    public string ActiveStatus => IsActive ? "Active" : "Inactive";

    public DateTime? LastTestedAt { get; set; }

    public string LastTestStatus { get; set; } = string.Empty;
}
