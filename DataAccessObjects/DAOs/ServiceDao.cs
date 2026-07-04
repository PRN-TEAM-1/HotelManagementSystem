using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class ServiceDao
{
    public async Task<Service?> GetByIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);
    }

    public async Task<Service?> GetByNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return null;
        }

        var normalizedName = serviceName.Trim();

        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServiceName == normalizedName, cancellationToken);
    }

    public async Task<List<Service>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AsNoTracking()
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Service>> GetByStatusAsync(ServiceStatus status, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AsNoTracking()
            .Where(s => s.Status == status)
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Service>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return new List<Service>();
        }

        var normalizedCategory = category.Trim();

        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AsNoTracking()
            .Where(s => s.Category == normalizedCategory)
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Service>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync(cancellationToken);
        }

        var term = searchTerm.Trim().ToLower();

        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AsNoTracking()
            .Where(s => s.ServiceName.ToLower().Contains(term) || s.Category.ToLower().Contains(term))
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Service> AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        await using var context = DbContextFactory.CreateDbContext();

        service.CreatedAt = DateTime.Now;
        service.UpdatedAt = DateTime.Now;

        context.Services.Add(service);
        await context.SaveChangesAsync(cancellationToken);

        return service;
    }

    public async Task<Service> UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        await using var context = DbContextFactory.CreateDbContext();

        service.UpdatedAt = DateTime.Now;

        context.Services.Update(service);
        await context.SaveChangesAsync(cancellationToken);

        return service;
    }

    public async Task<bool> DeleteAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var service = await context.Services.FindAsync(new object?[] { serviceId }, cancellationToken);

        if (service is null)
        {
            return false;
        }

        context.Services.Remove(service);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExistsByIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services.AnyAsync(s => s.ServiceId == serviceId, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return false;
        }

        var normalizedName = serviceName.Trim();

        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AnyAsync(s => s.ServiceName == normalizedName, cancellationToken);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string serviceName, int excludeServiceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return false;
        }

        var normalizedName = serviceName.Trim();

        await using var context = DbContextFactory.CreateDbContext();

        return await context.Services
            .AnyAsync(s => s.ServiceName == normalizedName && s.ServiceId != excludeServiceId, cancellationToken);
    }

    public async Task<bool> HasServiceOrdersAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.ServiceOrders
            .AnyAsync(so => so.ServiceId == serviceId && so.Status != ServiceOrderStatus.Cancelled, cancellationToken);
    }
}
