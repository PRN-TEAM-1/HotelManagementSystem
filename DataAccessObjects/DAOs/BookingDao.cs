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

    public async Task<List<Booking>> GetRecentBookingsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .OrderByDescending(booking => booking.CreatedAt)
            .Take(Math.Max(1, count))
            .ToListAsync(cancellationToken);
    }
}
