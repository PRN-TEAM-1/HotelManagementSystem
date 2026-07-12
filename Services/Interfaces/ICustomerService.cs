using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface ICustomerService
{
    Task<ServiceResult<List<CustomerListItemDto>>> GetCustomersAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerListItemDto>> CreateCustomerAsync(
        CreateCustomerRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerListItemDto>> UpdateCustomerAsync(
        UpdateCustomerRequestDto request,
        CancellationToken cancellationToken = default);
}
