using BusinessObjects.DTOs;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class CheckoutQueryRepository : ICheckoutQueryRepository
{
    private readonly CheckoutDao _dao;

    public CheckoutQueryRepository(CheckoutDao? dao = null)
    {
        _dao = dao ?? new CheckoutDao();
    }

    public async Task<List<CheckoutCandidateDto>> GetCandidatesForCheckoutAsync(CancellationToken cancellationToken = default)
    {
        return await _dao.GetCandidatesForCheckoutAsync(cancellationToken);
    }

    public async Task<CheckoutCandidateDto?> GetCheckoutCandidateByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetCheckoutCandidateByBookingDetailIdAsync(bookingDetailId, cancellationToken);
    }
}
