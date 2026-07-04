using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface ICheckInService
{
    Task<ServiceResult<List<CheckInCandidateDto>>> GetCheckInCandidatesAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<CheckInCandidateDto>> GetCheckInCandidateByIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<ServiceResult<CheckRecordDto>> CheckInAsync(CheckInRequestDto request, int currentUserId, CancellationToken cancellationToken = default);

    Task<ServiceResult<CheckRecordDto>> GetCheckRecordAsync(int checkRecordId, CancellationToken cancellationToken = default);
}
