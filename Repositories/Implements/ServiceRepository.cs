using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class ServiceRepository : IServiceRepository
{
    private readonly ServiceDao _dao;

    public ServiceRepository(ServiceDao? dao = null)
    {
        _dao = dao ?? new ServiceDao();
    }

    public async Task<Service?> GetByIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByIdAsync(serviceId, cancellationToken);
    }

    public async Task<Service?> GetByNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByNameAsync(serviceName, cancellationToken);
    }

    public async Task<List<Service>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dao.GetAllAsync(cancellationToken);
    }

    public async Task<List<Service>> GetByStatusAsync(ServiceStatus status, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByStatusAsync(status, cancellationToken);
    }

    public async Task<List<Service>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByCategoryAsync(category, cancellationToken);
    }

    public async Task<List<Service>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dao.SearchAsync(searchTerm, cancellationToken);
    }

    public async Task<Service> AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        return await _dao.AddAsync(service, cancellationToken);
    }

    public async Task<Service> UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);

        return await _dao.UpdateAsync(service, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        return await _dao.DeleteAsync(serviceId, cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        return await _dao.ExistsByIdAsync(serviceId, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return await _dao.ExistsByNameAsync(serviceName, cancellationToken);
    }

    public async Task<bool> ExistsByNameExcludingIdAsync(string serviceName, int excludeServiceId, CancellationToken cancellationToken = default)
    {
        return await _dao.ExistsByNameExcludingIdAsync(serviceName, excludeServiceId, cancellationToken);
    }

    public async Task<bool> HasServiceOrdersAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        return await _dao.HasServiceOrdersAsync(serviceId, cancellationToken);
    }
}
