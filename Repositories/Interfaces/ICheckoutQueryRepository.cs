using BusinessObjects.DTOs;

namespace Repositories.Interfaces;

public interface ICheckoutQueryRepository
{
    Task<List<CheckoutCandidateDto>> GetCandidatesForCheckoutAsync(CancellationToken cancellationToken = default);

    Task<CheckoutCandidateDto?> GetCheckoutCandidateByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);
}
