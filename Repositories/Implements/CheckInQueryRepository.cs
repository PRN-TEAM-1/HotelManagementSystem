using BusinessObjects.DTOs;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class CheckInQueryRepository : ICheckInQueryRepository
{
    private readonly CheckInQueryDao _dao;

    public CheckInQueryRepository(CheckInQueryDao? dao = null)
    {
        _dao = dao ?? new CheckInQueryDao();
    }

    public async Task<List<CheckInCandidateDto>> GetCandidatesForCheckInAsync(CancellationToken cancellationToken = default)
    {
        return await _dao.GetCandidatesForCheckInAsync(cancellationToken);
    }

    public async Task<CheckInCandidateDto?> GetCheckInCandidateByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetCheckInCandidateByBookingDetailIdAsync(bookingDetailId, cancellationToken);
    }
}
