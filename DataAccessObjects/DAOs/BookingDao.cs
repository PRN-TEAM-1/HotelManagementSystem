using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class BookingDao
{
    public async Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);

        await using var context = DbContextFactory.CreateDbContext();

        booking.CreatedAt = DateTime.Now;
        booking.UpdatedAt = DateTime.Now;

        context.Bookings.Add(booking);
        await context.SaveChangesAsync(cancellationToken);
        return booking;
    }

    public async Task<List<BookingDetail>> AddBookingDetailsAsync(
        IEnumerable<BookingDetail> details,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(details);

        await using var context = DbContextFactory.CreateDbContext();

        context.BookingDetails.AddRange(details);
        await context.SaveChangesAsync(cancellationToken);
        return details.ToList();
    }

    public async Task<Booking> CreateBookingWithTransactionAsync(Booking booking, IEnumerable<BookingDetail> details, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);
        ArgumentNullException.ThrowIfNull(details);

        await using var context = DbContextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            booking.CreatedAt = DateTime.Now;
            booking.UpdatedAt = DateTime.Now;

            context.Bookings.Add(booking);
            await context.SaveChangesAsync(cancellationToken);

            foreach (var detail in details)
            {
                detail.BookingId = booking.BookingId;
                detail.CreatedAt = DateTime.Now;
                detail.UpdatedAt = DateTime.Now;
            }

            context.BookingDetails.AddRange(details);
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return booking;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<List<Booking>> GetRecentBookingsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Customer)
            .Include(booking => booking.BookingDetails)
                .ThenInclude(detail => detail.Room)
            .OrderByDescending(booking => booking.CreatedAt)
            .Take(Math.Max(1, count))
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, decimal>> GetRoomPricesAsync(
        IEnumerable<int> roomIds,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var ids = roomIds.Distinct().ToList();
        return await context.Rooms
            .AsNoTracking()
            .Include(room => room.RoomType)
            .Where(room => ids.Contains(room.RoomId))
            .ToDictionaryAsync(
                room => room.RoomId,
                room => room.RoomType?.BasePrice ?? 0m,
                cancellationToken);
    }

    public async Task<bool> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var booking = await context.Bookings
            .Include(b => b.BookingDetails)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);

        if (booking is null || booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
        {
            return false;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.Now;

        foreach (var detail in booking.BookingDetails)
        {
            if (detail.Status == BookingDetailStatus.Reserved)
            {
                detail.Status = BookingDetailStatus.Cancelled;
                detail.UpdatedAt = DateTime.Now;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<int>> GetOverlappingRoomIdsAsync(
        IEnumerable<int> roomIds,
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var ids = roomIds.Distinct().ToList();
        return await context.BookingDetails
            .AsNoTracking()
            .Where(detail => ids.Contains(detail.RoomId))
            .Where(detail => detail.Status != BookingDetailStatus.Cancelled && detail.Status != BookingDetailStatus.CheckedOut)
            .Where(detail => detail.CheckInDate < checkOut && detail.CheckOutDate > checkIn)
            .Select(detail => detail.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}


