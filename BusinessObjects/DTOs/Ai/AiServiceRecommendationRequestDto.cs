namespace BusinessObjects.DTOs;

public sealed class AiServiceRecommendationRequestDto
{
    public int BookingDetailId { get; set; }

    public string? GuestPreference { get; set; }

    public int MaxRecommendations { get; set; } = 3;
}
