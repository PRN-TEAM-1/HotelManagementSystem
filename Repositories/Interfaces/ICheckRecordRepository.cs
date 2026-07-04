using BusinessObjects.Entities;
using BusinessObjects.Enums;

namespace Repositories.Interfaces;

public interface ICheckRecordRepository
{
    Task<CheckRecord?> GetByIdAsync(int checkRecordId, CancellationToken cancellationToken = default);

    Task<CheckRecord?> GetByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<List<CheckRecord>> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);

    Task<CheckRecord> AddAsync(CheckRecord checkRecord, CancellationToken cancellationToken = default);

    Task<CheckRecord> UpdateAsync(CheckRecord checkRecord, CancellationToken cancellationToken = default);

    Task<bool> ExistsByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);
}
