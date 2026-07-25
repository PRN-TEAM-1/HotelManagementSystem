using BusinessObjects.DTOs;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class AiRecommendationRepository : IAiRecommendationRepository
{
    private readonly AiRecommendationDao _dao;

    public AiRecommendationRepository(AiRecommendationDao? dao = null)
    {
        _dao = dao ?? new AiRecommendationDao();
    }

    public async Task<AiServiceRecommendationContextDto?> GetServiceRecommendationContextAsync(
        int bookingDetailId,
        CancellationToken cancellationToken = default)
    {
        return await _dao.GetServiceRecommendationContextAsync(bookingDetailId, cancellationToken);
    }
}
