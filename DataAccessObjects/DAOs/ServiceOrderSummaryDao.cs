using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class ServiceOrderSummaryDao
{
    public async Task<ServiceOrderSummaryDto?> GetSummaryByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var serviceOrders = await context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.BookingDetailId == bookingDetailId && so.Status != ServiceOrderStatus.Cancelled)
            .Join(
                context.Services.AsNoTracking(),
                so => so.ServiceId,
                s => s.ServiceId,
                (so, s) => new ServiceOrderListItemDto
                {
                    ServiceOrderId = so.ServiceOrderId,
                    BookingDetailId = so.BookingDetailId,
                    ServiceId = so.ServiceId,
                    ServiceName = s.ServiceName,
                    Quantity = so.Quantity,
                    UnitPrice = so.UnitPrice,
                    TotalPrice = so.TotalPrice,
                    OrderDate = so.OrderDate,
                    Status = so.Status.ToString(),
                    Note = so.Note
                })
            .OrderBy(dto => dto.OrderDate)
            .ToListAsync(cancellationToken);

        if (serviceOrders.Count == 0)
        {
            return null;
        }

        return new ServiceOrderSummaryDto
        {
            BookingDetailId = bookingDetailId,
            TotalServiceAmount = serviceOrders.Sum(so => so.TotalPrice),
            ServiceOrderCount = serviceOrders.Count,
            ServiceOrders = serviceOrders
        };
    }

    public async Task<decimal> GetTotalServiceAmountByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.BookingDetailId == bookingDetailId && so.Status != ServiceOrderStatus.Cancelled)
            .SumAsync(so => so.TotalPrice, cancellationToken);
    }

    public async Task<decimal> GetTotalServiceAmountByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AsNoTracking()
            .Where(so => context.BookingDetails
                .Where(bd => bd.BookingId == bookingId)
                .Select(bd => bd.BookingDetailId)
                .Contains(so.BookingDetailId) && so.Status != ServiceOrderStatus.Cancelled)
            .SumAsync(so => so.TotalPrice, cancellationToken);
    }
}
