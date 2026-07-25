using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IAiServiceRecommendationService
{
    Task<ServiceResult<AiServiceRecommendationResponseDto>> GetRecommendationsAsync(
        AiServiceRecommendationRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);
}
