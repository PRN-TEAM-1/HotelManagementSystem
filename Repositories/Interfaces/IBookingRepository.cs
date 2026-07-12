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
}
