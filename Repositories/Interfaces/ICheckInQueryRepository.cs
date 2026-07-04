using BusinessObjects.DTOs;

namespace Repositories.Interfaces;

public interface ICheckInQueryRepository
{
    Task<List<CheckInCandidateDto>> GetCandidatesForCheckInAsync(CancellationToken cancellationToken = default);

    Task<CheckInCandidateDto?> GetCheckInCandidateByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);
}
