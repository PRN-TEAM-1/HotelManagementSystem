using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;

namespace Repositories.Interfaces;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(int serviceId, CancellationToken cancellationToken = default);

    Task<Service?> GetByNameAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<List<Service>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<Service>> GetByStatusAsync(ServiceStatus status, CancellationToken cancellationToken = default);

    Task<List<Service>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    Task<List<Service>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    Task<Service> AddAsync(Service service, CancellationToken cancellationToken = default);

    Task<Service> UpdateAsync(Service service, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int serviceId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdAsync(int serviceId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameExcludingIdAsync(string serviceName, int excludeServiceId, CancellationToken cancellationToken = default);

    Task<bool> HasServiceOrdersAsync(int serviceId, CancellationToken cancellationToken = default);
}
