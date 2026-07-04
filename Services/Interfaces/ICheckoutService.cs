using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface ICheckoutService
{
    Task<ServiceResult<List<CheckoutCandidateDto>>> GetCheckoutCandidatesAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<CheckoutCandidateDto>> GetCheckoutCandidateByIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<ServiceResult<CheckoutResultDto>> CheckoutAsync(CheckoutRequestDto request, int currentUserId, CancellationToken cancellationToken = default);
}
