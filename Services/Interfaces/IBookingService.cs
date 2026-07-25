using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IBookingService
{
    Task<ServiceResult<BookingSummaryDto>> CreateBookingAsync(
        CreateBookingRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<BookingSummaryDto>>> GetRecentBookingsAsync(
        CurrentSessionDto? currentUser,
        int count = 10,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> CancelBookingAsync(
        int bookingId,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);
}

