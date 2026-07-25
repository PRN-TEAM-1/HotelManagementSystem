namespace BusinessObjects.DTOs;

public sealed class AiRecommendedServiceDto
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int SuggestedQuantity { get; set; }

    public decimal EstimatedAmount => UnitPrice * SuggestedQuantity;

    public decimal Confidence { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string UpsellMessage { get; set; } = string.Empty;
}
