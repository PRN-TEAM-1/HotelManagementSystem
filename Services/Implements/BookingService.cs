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

public sealed class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingService(IBookingRepository? bookingRepository = null)
    {
        _bookingRepository = bookingRepository ?? new BookingRepository();
    }

    public async Task<ServiceResult<BookingSummaryDto>> CreateBookingAsync(
        CreateBookingRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageBookings<BookingSummaryDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomerId <= 0 || request.CreatedByUserId <= 0)
        {
            return ServiceResult<BookingSummaryDto>.Failure(ErrorMessages.InvalidInput);
        }

        if (request.RoomIds.Count == 0)
        {
            return ServiceResult<BookingSummaryDto>.Failure(ErrorMessages.ValidationFailed, "At least one room is required.");
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            return ServiceResult<BookingSummaryDto>.Failure(ErrorMessages.InvalidDateRange);
        }

        var overlappingRoomIds = await _bookingRepository.GetOverlappingRoomIdsAsync(
            request.RoomIds,
            request.CheckInDate,
            request.CheckOutDate,
            cancellationToken);

        if (overlappingRoomIds.Count > 0)
        {
            return ServiceResult<BookingSummaryDto>.Failure(
                ErrorMessages.DuplicateRecord,
                $"Room(s) with ID {string.Join(", ", overlappingRoomIds)} is already reserved or occupied for the selected dates.");
        }

        try
        {
            // Validate user role and room operational status
            await using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var user = await dbContext.Users.Include(u => u.Role)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == request.CreatedByUserId && u.Status == UserStatus.Active, cancellationToken);

                if (user is null)
                {
                    return ServiceResult<BookingSummaryDto>.Failure(ErrorMessages.Unauthorized);
                }

                if (user.Role?.Name is not (RoleName.Admin or RoleName.Manager or RoleName.Receptionist))
                {
                    return ServiceResult<BookingSummaryDto>.Failure(ErrorMessages.Forbidden);
                }

                var rooms = await dbContext.Rooms
                    .AsNoTracking()
                    .Where(r => request.RoomIds.Contains(r.RoomId))
                    .ToListAsync(cancellationToken);

                var nonAvailableRooms = rooms
                    .Where(r => r.Status != RoomOperationalStatus.Available)
                    .Select(r => r.RoomNumber)
                    .ToList();

                if (nonAvailableRooms.Count > 0)
                {
                    return ServiceResult<BookingSummaryDto>.Failure(
                        ErrorMessages.BusinessRuleViolation,
                        $"Room(s) {string.Join(", ", nonAvailableRooms)} is not Available (currently Maintenance, Inactive, or Cleaning).");
                }
            }


            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                CreatedByUserId = request.CreatedByUserId,
                BookingDate = DateTime.Today,
                Status = BookingStatus.Confirmed,
                Note = request.Note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var roomPrices = await _bookingRepository.GetRoomPricesAsync(request.RoomIds, cancellationToken);
            var numberOfNights = Math.Max(1, (int)(request.CheckOutDate.Date - request.CheckInDate.Date).TotalDays);

            var details = request.RoomIds.Select(roomId =>
            {
                var price = roomPrices.TryGetValue(roomId, out var p) ? p : 0m;
                var total = price * numberOfNights;

                return new BookingDetail
                {
                    RoomId = roomId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    RoomPrice = price,
                    NumberOfNights = numberOfNights,
                    RoomTotal = total,
                    Status = BookingDetailStatus.Reserved,
                    Note = request.Note,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }).ToList();

            var createdBooking = await _bookingRepository.CreateBookingWithTransactionAsync(booking, details, cancellationToken);
            var bookingTotal = details.Sum(d => d.RoomTotal);

            return ServiceResult<BookingSummaryDto>.Success(new BookingSummaryDto
            {
                BookingId = createdBooking.BookingId,
                CustomerName = string.Empty,
                RoomNumbers = string.Join(", ", request.RoomIds),
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                Status = createdBooking.Status.ToString(),
                RoomTotal = bookingTotal
            }, "Booking created successfully.");
        }

        catch (Exception ex)
        {
            return ServiceResult<BookingSummaryDto>.Failure(ErrorMessages.SystemError, ex.Message);
        }
    }

    public async Task<ServiceResult<List<BookingSummaryDto>>> GetRecentBookingsAsync(
        CurrentSessionDto? currentUser,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageBookings<List<BookingSummaryDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var bookings = await _bookingRepository.GetRecentBookingsAsync(count, cancellationToken);
            return ServiceResult<List<BookingSummaryDto>>.Success(bookings.Select(booking =>
            {
                var roomNumbers = string.Join(", ", booking.BookingDetails
                    .Where(bd => bd.Room != null)
                    .Select(bd => bd.Room!.RoomNumber));

                var checkIn = booking.BookingDetails.FirstOrDefault()?.CheckInDate ?? booking.BookingDate;
                var checkOut = booking.BookingDetails.FirstOrDefault()?.CheckOutDate ?? booking.BookingDate;
                var total = booking.BookingDetails.Sum(bd => bd.RoomTotal);

                return new BookingSummaryDto
                {
                    BookingId = booking.BookingId,
                    CustomerName = booking.Customer?.FullName ?? string.Empty,
                    RoomNumbers = roomNumbers,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    Status = booking.Status.ToString(),
                    RoomTotal = total
                };
            }).ToList());
        }
        catch
        {
            return ServiceResult<List<BookingSummaryDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<bool>> CancelBookingAsync(
        int bookingId,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageBookings<bool>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        if (bookingId <= 0)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var success = await _bookingRepository.CancelBookingAsync(bookingId, cancellationToken);
            if (success)
            {
                return ServiceResult<bool>.Success(true, "Booking cancelled successfully.");
            }
            return ServiceResult<bool>.Failure("Unable to cancel booking. It may have checked-in rooms or already be cancelled/completed/no-show.");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.SystemError, ex.Message);
        }
    }

    public async Task<ServiceResult<bool>> MarkNoShowAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        if (bookingId <= 0)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var success = await _bookingRepository.MarkNoShowAsync(bookingId, cancellationToken);
            if (success)
            {
                return ServiceResult<bool>.Success(true, "Booking marked as No-Show successfully.");
            }
            return ServiceResult<bool>.Failure("Unable to mark booking as No-Show. It may have checked-in rooms or already be cancelled/completed/no-show.");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Failure(ErrorMessages.SystemError, ex.Message);
        }
    }

    private static ServiceResult<T>? EnsureCanManageBookings<T>(CurrentSessionDto? currentUser)
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

