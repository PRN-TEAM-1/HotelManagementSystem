using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class CheckRecordDao
{
    public async Task<CheckRecord?> GetByIdAsync(int checkRecordId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.CheckRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.CheckRecordId == checkRecordId, cancellationToken);
    }

    public async Task<CheckRecord?> GetByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.CheckRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(cr => cr.BookingDetailId == bookingDetailId, cancellationToken);
    }

    public async Task<List<CheckRecord>> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.CheckRecords
            .AsNoTracking()
            .Where(cr => context.BookingDetails
                .Where(bd => bd.BookingId == bookingId)
                .Select(bd => bd.BookingDetailId)
                .Contains(cr.BookingDetailId))
            .ToListAsync(cancellationToken);
    }

    public async Task<CheckRecord> AddAsync(CheckRecord checkRecord, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkRecord);

        await using var context = DbContextFactory.CreateDbContext();

        checkRecord.CreatedAt = DateTime.Now;
        checkRecord.UpdatedAt = DateTime.Now;

        context.CheckRecords.Add(checkRecord);
        await context.SaveChangesAsync(cancellationToken);

        return checkRecord;
    }

    public async Task<CheckRecord> UpdateAsync(CheckRecord checkRecord, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkRecord);

        await using var context = DbContextFactory.CreateDbContext();

        checkRecord.UpdatedAt = DateTime.Now;

        context.CheckRecords.Update(checkRecord);
        await context.SaveChangesAsync(cancellationToken);

        return checkRecord;
    }

    public async Task<bool> ExistsByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.CheckRecords
            .AnyAsync(cr => cr.BookingDetailId == bookingDetailId, cancellationToken);
    }
}
