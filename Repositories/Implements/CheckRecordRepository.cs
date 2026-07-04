using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class CheckRecordRepository : ICheckRecordRepository
{
    private readonly CheckRecordDao _dao;

    public CheckRecordRepository(CheckRecordDao? dao = null)
    {
        _dao = dao ?? new CheckRecordDao();
    }

    public async Task<CheckRecord?> GetByIdAsync(int checkRecordId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByIdAsync(checkRecordId, cancellationToken);
    }

    public async Task<CheckRecord?> GetByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByBookingDetailIdAsync(bookingDetailId, cancellationToken);
    }

    public async Task<List<CheckRecord>> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByBookingIdAsync(bookingId, cancellationToken);
    }

    public async Task<CheckRecord> AddAsync(CheckRecord checkRecord, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkRecord);

        return await _dao.AddAsync(checkRecord, cancellationToken);
    }

    public async Task<CheckRecord> UpdateAsync(CheckRecord checkRecord, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkRecord);

        return await _dao.UpdateAsync(checkRecord, cancellationToken);
    }

    public async Task<bool> ExistsByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _dao.ExistsByBookingDetailIdAsync(bookingDetailId, cancellationToken);
    }
}
