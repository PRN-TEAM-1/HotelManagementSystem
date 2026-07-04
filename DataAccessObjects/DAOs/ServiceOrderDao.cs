using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class ServiceOrderDao
{
    public async Task<ServiceOrder?> GetByIdAsync(int serviceOrderId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(so => so.ServiceOrderId == serviceOrderId, cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.BookingDetailId == bookingDetailId)
            .OrderBy(so => so.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByBookingDetailIdExcludingCancelledAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.BookingDetailId == bookingDetailId && so.Status != ServiceOrderStatus.Cancelled)
            .OrderBy(so => so.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.ServiceId == serviceId && so.Status != ServiceOrderStatus.Cancelled)
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceOrder> AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceOrder);

        await using var context = DbContextFactory.CreateDbContext();

        serviceOrder.CreatedAt = DateTime.Now;
        serviceOrder.UpdatedAt = DateTime.Now;

        context.ServiceOrders.Add(serviceOrder);
        await context.SaveChangesAsync(cancellationToken);

        return serviceOrder;
    }

    public async Task<ServiceOrder> UpdateAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceOrder);

        await using var context = DbContextFactory.CreateDbContext();

        serviceOrder.UpdatedAt = DateTime.Now;

        context.ServiceOrders.Update(serviceOrder);
        await context.SaveChangesAsync(cancellationToken);

        return serviceOrder;
    }

    public async Task<bool> CancelAsync(int serviceOrderId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var serviceOrder = await context.ServiceOrders.FindAsync(new object?[] { serviceOrderId }, cancellationToken);

        if (serviceOrder is null || serviceOrder.Status == ServiceOrderStatus.Cancelled)
        {
            return false;
        }

        serviceOrder.Status = ServiceOrderStatus.Cancelled;
        serviceOrder.UpdatedAt = DateTime.Now;

        context.ServiceOrders.Update(serviceOrder);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExistsAsync(int serviceOrderId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AnyAsync(so => so.ServiceOrderId == serviceOrderId, cancellationToken);
    }
}
