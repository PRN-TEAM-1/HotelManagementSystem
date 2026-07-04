using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceCatalogService(IServiceRepository? serviceRepository = null)
    {
        _serviceRepository = serviceRepository ?? new ServiceRepository();
    }

    public async Task<ServiceResult<ServiceDto>> GetServiceByIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        if (serviceId <= 0)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);

            if (service is null)
            {
                return ServiceResult<ServiceDto>.Failure(ErrorMessages.NotFound);
            }

            var dto = MapToServiceDto(service);
            return ServiceResult<ServiceDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<ServiceDto>> GetServiceByNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.ValidationFailed);
        }

        try
        {
            var service = await _serviceRepository.GetByNameAsync(serviceName, cancellationToken);

            if (service is null)
            {
                return ServiceResult<ServiceDto>.Failure(ErrorMessages.NotFound);
            }

            var dto = MapToServiceDto(service);
            return ServiceResult<ServiceDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<ServiceListItemDto>>> GetAllServicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var services = await _serviceRepository.GetAllAsync(cancellationToken);
            var dtos = services.Select(MapToServiceListItemDto).ToList();
            return ServiceResult<List<ServiceListItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ServiceListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<ServiceListItemDto>>> GetActiveServicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var services = await _serviceRepository.GetByStatusAsync(ServiceStatus.Active, cancellationToken);
            var dtos = services.Select(MapToServiceListItemDto).ToList();
            return ServiceResult<List<ServiceListItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ServiceListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<ServiceListItemDto>>> SearchServicesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return ServiceResult<List<ServiceListItemDto>>.Failure(ErrorMessages.ValidationFailed);
        }

        try
        {
            var services = await _serviceRepository.SearchAsync(searchTerm, cancellationToken);
            var dtos = services.Select(MapToServiceListItemDto).ToList();
            return ServiceResult<List<ServiceListItemDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ServiceListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<ServiceDto>> CreateServiceAsync(CreateServiceRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ServiceName))
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.ValidationFailed);
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.ValidationFailed);
        }

        if (request.Price <= 0)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.ValidationFailed);
        }

        try
        {
            // Check if service with the same name already exists
            var existingService = await _serviceRepository.GetByNameAsync(request.ServiceName, cancellationToken);
            if (existingService is not null)
            {
                return ServiceResult<ServiceDto>.Failure(ErrorMessages.DuplicateRecord);
            }

            // Create the service
            var service = new Service
            {
                ServiceName = request.ServiceName,
                Category = request.Category,
                Price = request.Price,
                Status = ServiceStatus.Active,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdService = await _serviceRepository.AddAsync(service, cancellationToken);

            var dto = MapToServiceDto(createdService);
            return ServiceResult<ServiceDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<ServiceDto>> UpdateServiceAsync(UpdateServiceRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ServiceId <= 0)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.InvalidInput);
        }

        if (string.IsNullOrWhiteSpace(request.ServiceName))
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.ValidationFailed);
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.ValidationFailed);
        }

        if (request.Price <= 0)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.ValidationFailed);
        }

        try
        {
            // Get the existing service
            var existingService = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (existingService is null)
            {
                return ServiceResult<ServiceDto>.Failure(ErrorMessages.NotFound);
            }

            // Check if another service with the same name already exists
            if (!existingService.ServiceName.Equals(request.ServiceName, StringComparison.OrdinalIgnoreCase))
            {
                var duplicateService = await _serviceRepository.GetByNameAsync(request.ServiceName, cancellationToken);
                if (duplicateService is not null)
                {
                    return ServiceResult<ServiceDto>.Failure(ErrorMessages.DuplicateRecord);
                }
            }

            // Update the service
            existingService.ServiceName = request.ServiceName;
            existingService.Category = request.Category;
            existingService.Price = request.Price;

            if (Enum.TryParse<ServiceStatus>(request.Status, ignoreCase: true, out var statusValue))
            {
                existingService.Status = statusValue;
            }

            existingService.UpdatedAt = DateTime.Now;

            var updatedService = await _serviceRepository.UpdateAsync(existingService, cancellationToken);

            var dto = MapToServiceDto(updatedService);
            return ServiceResult<ServiceDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ServiceDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<bool>> InactivateServiceAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        if (serviceId <= 0)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            // Get the service
            var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
            if (service is null)
            {
                return ServiceResult<bool>.Failure(ErrorMessages.NotFound);
            }

            // Update status to Inactive
            service.Status = ServiceStatus.Inactive;
            service.UpdatedAt = DateTime.Now;

            await _serviceRepository.UpdateAsync(service, cancellationToken);
            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.SystemError);
        }
    }

    private ServiceDto MapToServiceDto(Service service)
    {
        return new ServiceDto
        {
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            Category = service.Category,
            Price = service.Price,
            Status = service.Status.ToString(),
            CreatedAt = service.CreatedAt,
            UpdatedAt = service.UpdatedAt
        };
    }

    private ServiceListItemDto MapToServiceListItemDto(Service service)
    {
        return new ServiceListItemDto
        {
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            Category = service.Category,
            Price = service.Price,
            Status = service.Status.ToString()
        };
    }
}
