using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IServiceCatalogService
{
    Task<ServiceResult<ServiceDto>> GetServiceByIdAsync(int serviceId, CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceDto>> GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken = default);

    Task<ServiceResult<List<ServiceListItemDto>>> GetAllServicesAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<List<ServiceListItemDto>>> GetActiveServicesAsync(CancellationToken cancellationToken = default);

    Task<ServiceResult<List<ServiceListItemDto>>> SearchServicesAsync(string searchTerm, CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceDto>> CreateServiceAsync(
        CreateServiceRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceDto>> UpdateServiceAsync(
        UpdateServiceRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> InactivateServiceAsync(
        int serviceId,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default);
}
