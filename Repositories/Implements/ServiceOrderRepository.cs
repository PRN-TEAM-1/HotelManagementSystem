using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly ServiceOrderDao _dao;
    private readonly ServiceOrderSummaryDao _summaryDao;

    public ServiceOrderRepository(ServiceOrderDao? dao = null, ServiceOrderSummaryDao? summaryDao = null)
    {
        _dao = dao ?? new ServiceOrderDao();
        _summaryDao = summaryDao ?? new ServiceOrderSummaryDao();
    }

    public async Task<ServiceOrder?> GetByIdAsync(int serviceOrderId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByIdAsync(serviceOrderId, cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByBookingDetailIdAsync(bookingDetailId, cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByBookingDetailIdExcludingCancelledAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByBookingDetailIdExcludingCancelledAsync(bookingDetailId, cancellationToken);
    }

    public async Task<List<ServiceOrder>> GetByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetByServiceIdAsync(serviceId, cancellationToken);
    }

    public async Task<ServiceOrder> AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceOrder);

        return await _dao.AddAsync(serviceOrder, cancellationToken);
    }

    public async Task<ServiceOrder> UpdateAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceOrder);

        return await _dao.UpdateAsync(serviceOrder, cancellationToken);
    }

    public async Task<bool> CancelAsync(int serviceOrderId, CancellationToken cancellationToken = default)
    {
        return await _dao.CancelAsync(serviceOrderId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int serviceOrderId, CancellationToken cancellationToken = default)
    {
        return await _dao.ExistsAsync(serviceOrderId, cancellationToken);
    }

    public async Task<ServiceOrderSummaryDto?> GetSummaryByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _summaryDao.GetSummaryByBookingDetailIdAsync(bookingDetailId, cancellationToken);
    }

    public async Task<decimal> GetTotalServiceAmountByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _summaryDao.GetTotalServiceAmountByBookingDetailIdAsync(bookingDetailId, cancellationToken);
    }

    public async Task<decimal> GetTotalServiceAmountByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return await _summaryDao.GetTotalServiceAmountByBookingIdAsync(bookingId, cancellationToken);
    }
}
