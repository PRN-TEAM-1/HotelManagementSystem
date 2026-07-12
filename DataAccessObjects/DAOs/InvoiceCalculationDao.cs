using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class InvoiceCalculationDao
{
    public async Task<List<InvoiceCandidateDto>> GetInvoiceCandidatesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var bookings = await context.Bookings
            .AsNoTracking()
            .Where(booking => !context.Invoices.Any(invoice => invoice.BookingId == booking.BookingId))
            .Join(
                context.Customers.AsNoTracking(),
                booking => booking.CustomerId,
                customer => customer.CustomerId,
                (booking, customer) => new
                {
                    booking.BookingId,
                    booking.BookingDate,
                    BookingStatus = booking.Status,
                    CustomerName = customer.FullName
                })
            .OrderByDescending(booking => booking.BookingDate)
            .ToListAsync(cancellationToken);

        var candidates = new List<InvoiceCandidateDto>();

        foreach (var booking in bookings)
        {
            var candidate = await BuildCandidateAsync(
                booking.BookingId,
                booking.CustomerName,
                booking.BookingDate,
                booking.BookingStatus.ToString(),
                cancellationToken);

            if (candidate is not null)
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    public async Task<InvoiceCandidateDto?> GetInvoiceCandidateByBookingIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var booking = await context.Bookings
            .AsNoTracking()
            .Where(existingBooking => existingBooking.BookingId == bookingId)
            .Join(
                context.Customers.AsNoTracking(),
                existingBooking => existingBooking.CustomerId,
                customer => customer.CustomerId,
                (existingBooking, customer) => new
                {
                    existingBooking.BookingId,
                    existingBooking.BookingDate,
                    BookingStatus = existingBooking.Status,
                    CustomerName = customer.FullName
                })
            .FirstOrDefaultAsync(cancellationToken);

        if (booking is null)
        {
            return null;
        }

        var hasInvoice = await context.Invoices
            .AsNoTracking()
            .AnyAsync(invoice => invoice.BookingId == bookingId, cancellationToken);

        if (hasInvoice)
        {
            return null;
        }

        return await BuildCandidateAsync(
            booking.BookingId,
            booking.CustomerName,
            booking.BookingDate,
            booking.BookingStatus.ToString(),
            cancellationToken);
    }

    public async Task<bool> BookingExistsAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .AnyAsync(booking => booking.BookingId == bookingId, cancellationToken);
    }

    private static async Task<InvoiceCandidateDto?> BuildCandidateAsync(
        int bookingId,
        string customerName,
        DateTime bookingDate,
        string bookingStatus,
        CancellationToken cancellationToken)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var bookingDetails = await context.BookingDetails
            .AsNoTracking()
            .Where(detail => detail.BookingId == bookingId)
            .ToListAsync(cancellationToken);

        if (bookingDetails.Count == 0)
        {
            return null;
        }

        var isReady = bookingDetails.All(detail =>
            detail.Status is BookingDetailStatus.CheckedOut or BookingDetailStatus.Cancelled);

        if (!isReady)
        {
            return null;
        }

        var bookingDetailIds = bookingDetails
            .Select(detail => detail.BookingDetailId)
            .ToArray();

        var roomAmount = bookingDetails.Sum(detail => detail.RoomTotal);
        var serviceAmount = await context.ServiceOrders
            .AsNoTracking()
            .Where(order =>
                bookingDetailIds.Contains(order.BookingDetailId)
                && order.Status == ServiceOrderStatus.Ordered)
            .SumAsync(order => (decimal?)order.TotalPrice, cancellationToken) ?? 0m;

        return new InvoiceCandidateDto
        {
            BookingId = bookingId,
            CustomerName = customerName,
            BookingDate = bookingDate,
            BookingStatus = bookingStatus,
            RoomCount = bookingDetails.Count,
            RoomAmount = roomAmount,
            ServiceAmount = serviceAmount,
            EstimatedTotalAmount = roomAmount + serviceAmount
        };
    }
}
