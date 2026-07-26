using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;


namespace Services.Implements;

public sealed class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IBookingOperationRepository _bookingOperationRepository;
    private readonly IUserActivityService _userActivityService;

    public ServiceOrderService(
        IServiceOrderRepository? serviceOrderRepository = null,
        IServiceRepository? serviceRepository = null,
        IBookingOperationRepository? bookingOperationRepository = null,
        IUserActivityService? userActivityService = null)
    {
        _serviceOrderRepository = serviceOrderRepository ?? new ServiceOrderRepository();
        _serviceRepository = serviceRepository ?? new ServiceRepository();
        _bookingOperationRepository = bookingOperationRepository ?? new BookingOperationRepository();
        _userActivityService = userActivityService ?? new UserActivityService();
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
        catch (Exception)
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
        catch (Exception)
        {
            return ServiceResult<ServiceOrderSummaryDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<ServiceOrderListItemDto>> CreateServiceOrderAsync(ServiceOrderRequestDto request, CurrentSessionDto? currentUser, CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageServiceOrders<ServiceOrderListItemDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        ArgumentNullException.ThrowIfNull(request);

        var validation = ValidateCreateServiceOrderRequest(request);
        if (!validation.IsSuccess)
        {
            return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.ValidationFailed);
        }

        try
        {
            // Validate user role
            await using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var user = await dbContext.Users.Include(u => u.Role)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == currentUser!.UserId && u.Status == UserStatus.Active, cancellationToken);

                if (user is null)
                {
                    return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.Unauthorized);
                }

                if (user.Role?.Name is not (RoleName.Admin or RoleName.Manager or RoleName.Receptionist))
                {
                    return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.Forbidden);
                }
            }

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
                CreatedByUserId = currentUser!.UserId,
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

            await _userActivityService.RecordActivityAsync(
                currentUser,
                "ServiceOrderCreated",
                "ServiceOrder",
                createdOrder.ServiceOrderId.ToString(),
                $"Created service order #{createdOrder.ServiceOrderId} for booking detail #{request.BookingDetailId}.",
                cancellationToken: cancellationToken);

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
        catch (Exception)
        {
            return ServiceResult<ServiceOrderListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<bool>> CancelServiceOrderAsync(
        int serviceOrderId,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageServiceOrders<bool>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

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

            await _userActivityService.RecordActivityAsync(
                currentUser,
                "ServiceOrderCancelled",
                "ServiceOrder",
                serviceOrderId.ToString(),
                $"Cancelled service order #{serviceOrderId}.",
                cancellationToken: cancellationToken);

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception)
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
        catch (Exception)
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

    private static ServiceResult<T>? EnsureCanManageServiceOrders<T>(CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        if (currentUser.RoleName is not (RoleName.Admin or RoleName.Receptionist or RoleName.Manager))
        {
            return ServiceResult<T>.Failure(ErrorMessages.Forbidden);
        }

        return null;
    }
}
