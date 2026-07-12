using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IBookingService
{
    Task<ServiceResult<BookingSummaryDto>> CreateBookingAsync(
        CreateBookingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<BookingSummaryDto>>> GetRecentBookingsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);
}
