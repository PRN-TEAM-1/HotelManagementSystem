using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface ICustomerService
{
    Task<ServiceResult<List<CustomerListItemDto>>> GetCustomersAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerListItemDto>> CreateCustomerAsync(
        CreateCustomerRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<CustomerListItemDto>> UpdateCustomerAsync(
        UpdateCustomerRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<BookingSummaryDto>>> GetCustomerBookingsAsync(
        int customerId,
        CancellationToken cancellationToken = default);
}

