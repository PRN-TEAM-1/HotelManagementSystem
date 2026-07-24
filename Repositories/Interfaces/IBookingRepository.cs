using BusinessObjects.Entities;

namespace Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default);

    Task<List<Booking>> GetRecentBookingsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    Task<List<BookingDetail>> AddBookingDetailsAsync(
        IEnumerable<BookingDetail> details,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, decimal>> GetRoomPricesAsync(
        IEnumerable<int> roomIds,
        CancellationToken cancellationToken = default);

    Task<bool> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default);

    Task<List<int>> GetOverlappingRoomIdsAsync(
        IEnumerable<int> roomIds,
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken = default);
}


