using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;

namespace Repositories.Interfaces;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(int serviceOrderId, CancellationToken cancellationToken = default);

    Task<List<ServiceOrder>> GetByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<List<ServiceOrder>> GetByBookingDetailIdExcludingCancelledAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<List<ServiceOrder>> GetByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default);

    Task<ServiceOrder> AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default);

    Task<ServiceOrder> UpdateAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(int serviceOrderId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int serviceOrderId, CancellationToken cancellationToken = default);

    Task<ServiceOrderSummaryDto?> GetSummaryByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<decimal> GetTotalServiceAmountByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<decimal> GetTotalServiceAmountByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);
}
