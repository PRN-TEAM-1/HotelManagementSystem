using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class BookingRepository : IBookingRepository
{
    private readonly BookingDao _dao;

    public BookingRepository(BookingDao? dao = null)
    {
        _dao = dao ?? new BookingDao();
    }

    public Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);
        return _dao.AddAsync(booking, cancellationToken);
    }

    public Task<List<BookingDetail>> AddBookingDetailsAsync(IEnumerable<BookingDetail> details, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(details);
        return _dao.AddBookingDetailsAsync(details, cancellationToken);
    }

    public Task<List<Booking>> GetRecentBookingsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return _dao.GetRecentBookingsAsync(count, cancellationToken);
    }
}
