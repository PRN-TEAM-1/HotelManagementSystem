using BusinessObjects.DTOs;

namespace Repositories.Interfaces;

public interface IAiRecommendationRepository
{
    Task<AiServiceRecommendationContextDto?> GetServiceRecommendationContextAsync(
        int bookingDetailId,
        CancellationToken cancellationToken = default);
}
