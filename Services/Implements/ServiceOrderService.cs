using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IBookingOperationRepository _bookingOperationRepository;

    public ServiceOrderService(
        IServiceOrderRepository? serviceOrderRepository = null,
        IServiceRepository? serviceRepository = null,
        IBookingOperationRepository? bookingOperationRepository = null)
    {
        _serviceOrderRepository = serviceOrderRepository ?? new ServiceOrderRepository();
        _serviceRepository = serviceRepository ?? new ServiceRepository();
        _bookingOperationRepository = bookingOperationRepository ?? new BookingOperationRepository();
    }

    public async Task<ServiceResult<List<ServiceOrderListItemDto>>> GetServiceOrdersByBookingDetailAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        if (bookingDetailId <= 0)
        {
            return ServiceResult<List<ServiceOrderListItemDto>>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var summary = await _serviceOrderRepository.GetSummaryByBookingDetailIdAsync(bookingDetailId, cancellationToken);
            var orders = summary?.ServiceOrders ?? new List<ServiceOrderListItemDto>();
            return ServiceResult<List<ServiceOrderListItemDto>>.Success(orders);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<ServiceOrderListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<ServiceOrderSummaryDto>> GetServiceOrderSummaryAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        if (bookingDetailId <= 0)
        {
            return ServiceResult<ServiceOrderSummaryDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var summary = await _serviceOrderRepository.GetSummaryByBookingDetailIdAsync(bookingDetailId, cancellationToken);

            if (summary is null)
            {
                summary = new ServiceOrderSummaryDto
                {
                    BookingDetailId = bookingDetailId,
                    TotalServiceAmount = 0,
                    ServiceOrderCount = 0,
                    ServiceOrders = new List<ServiceOrderListItemDto>()
                };
            }

            return ServiceResult<ServiceOrderSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return ServiceResult<ServiceOrderSummaryDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<ServiceOrderListItemDto>> CreateServiceOrderAsync(ServiceOrderRequestDto request, int currentUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = ValidateCreateServiceOrderRequest(request);
        if (!validation.IsSuccess)
        {
            return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.ValidationFailed);
        }

        if (currentUserId <= 0)
        {
            return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            // Get booking detail
            var bookingDetail = await _bookingOperationRepository.GetBookingDetailByIdAsync(request.BookingDetailId, cancellationToken);
            if (bookingDetail is null)
            {
                return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.NotFound);
            }

            // Check if booking detail is CheckedIn
            if (bookingDetail.Status != BookingDetailStatus.CheckedIn)
            {
                return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.BusinessRuleViolation);
            }

            // Get service
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
            if (service is null)
            {
                return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.NotFound);
            }

            // Check if service is active
            if (service.Status != ServiceStatus.Active)
            {
                return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.BusinessRuleViolation);
            }

            // Create service order
            var totalPrice = request.Quantity * service.Price;
            var serviceOrder = new ServiceOrder
            {
                BookingDetailId = request.BookingDetailId,
                ServiceId = request.ServiceId,
                CreatedByUserId = currentUserId,
                Quantity = request.Quantity,
                UnitPrice = service.Price,
                TotalPrice = totalPrice,
                OrderDate = DateTime.Now,
                Status = ServiceOrderStatus.Ordered,
                Note = request.Note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdOrder = await _serviceOrderRepository.AddAsync(serviceOrder, cancellationToken);

            var dto = new ServiceOrderListItemDto
            {
                ServiceOrderId = createdOrder.ServiceOrderId,
                BookingDetailId = createdOrder.BookingDetailId,
                ServiceId = createdOrder.ServiceId,
                ServiceName = service.ServiceName,
                Quantity = createdOrder.Quantity,
                UnitPrice = createdOrder.UnitPrice,
                TotalPrice = createdOrder.TotalPrice,
                OrderDate = createdOrder.OrderDate,
                Status = createdOrder.Status.ToString(),
                Note = createdOrder.Note
            };

            return ServiceResult<ServiceOrderListItemDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<bool>> CancelServiceOrderAsync(int serviceOrderId, CancellationToken cancellationToken = default)
    {
        if (serviceOrderId <= 0)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var success = await _serviceOrderRepository.CancelAsync(serviceOrderId, cancellationToken);

            if (!success)
            {
                return ServiceResult<bool>.Failure(ErrorMessages.NotFound);
            }

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<decimal>> GetServiceOrderTotalByBookingDetailAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        if (bookingDetailId <= 0)
        {
            return ServiceResult<decimal>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var total = await _serviceOrderRepository.GetTotalServiceAmountByBookingDetailIdAsync(bookingDetailId, cancellationToken);
            return ServiceResult<decimal>.Success(total);
        }
        catch (Exception ex)
        {
            return ServiceResult<decimal>.Failure(ErrorMessages.SystemError);
        }
    }

    private ServiceResult<bool> ValidateCreateServiceOrderRequest(ServiceOrderRequestDto request)
    {
        if (request.BookingDetailId <= 0)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.InvalidInput);
        }

        if (request.ServiceId <= 0)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.InvalidInput);
        }

        if (request.Quantity <= 0)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.ValidationFailed);
        }

        return ServiceResult<bool>.Success(true);
    }
}
