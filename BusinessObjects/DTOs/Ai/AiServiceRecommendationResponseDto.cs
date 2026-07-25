namespace BusinessObjects.DTOs;

public sealed class AiServiceRecommendationResponseDto
{
    public string ProviderName { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<AiRecommendedServiceDto> Recommendations { get; set; } = new();
}
