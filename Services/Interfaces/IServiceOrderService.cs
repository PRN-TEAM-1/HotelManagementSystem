using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IServiceOrderService
{
    Task<ServiceResult<List<ServiceOrderListItemDto>>> GetServiceOrdersByBookingDetailAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceOrderSummaryDto>> GetServiceOrderSummaryAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<ServiceResult<ServiceOrderListItemDto>> CreateServiceOrderAsync(ServiceOrderRequestDto request, int currentUserId, CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> CancelServiceOrderAsync(int serviceOrderId, CancellationToken cancellationToken = default);

    Task<ServiceResult<decimal>> GetServiceOrderTotalByBookingDetailAsync(int bookingDetailId, CancellationToken cancellationToken = default);
}
