using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

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
        CancellationToken cancellationToken = default)
    {
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

        try
        {
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

            var createdBooking = await _bookingRepository.AddAsync(booking, cancellationToken);

            var details = request.RoomIds.Select(roomId => new BookingDetail
            {
                BookingId = createdBooking.BookingId,
                RoomId = roomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                RoomPrice = 0,
                NumberOfNights = (int)(request.CheckOutDate - request.CheckInDate).TotalDays,
                RoomTotal = 0,
                Status = BookingDetailStatus.Reserved,
                Note = request.Note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await _bookingRepository.AddBookingDetailsAsync(details, cancellationToken);

            return ServiceResult<BookingSummaryDto>.Success(new BookingSummaryDto
            {
                BookingId = createdBooking.BookingId,
                CustomerName = string.Empty,
                RoomNumbers = string.Join(", ", request.RoomIds),
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                Status = createdBooking.Status.ToString(),
                RoomTotal = 0
            }, "Booking created successfully.");
        }
        catch
        {
            return ServiceResult<BookingSummaryDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<BookingSummaryDto>>> GetRecentBookingsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bookings = await _bookingRepository.GetRecentBookingsAsync(count, cancellationToken);
            return ServiceResult<List<BookingSummaryDto>>.Success(bookings.Select(booking => new BookingSummaryDto
            {
                BookingId = booking.BookingId,
                CustomerName = string.Empty,
                RoomNumbers = string.Empty,
                CheckInDate = booking.BookingDate,
                CheckOutDate = booking.BookingDate,
                Status = booking.Status.ToString(),
                RoomTotal = 0
            }).ToList());
        }
        catch
        {
            return ServiceResult<List<BookingSummaryDto>>.Failure(ErrorMessages.SystemError);
        }
    }
}
