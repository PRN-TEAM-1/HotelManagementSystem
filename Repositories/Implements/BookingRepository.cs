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

    public Task<Booking> CreateBookingWithTransactionAsync(Booking booking, IEnumerable<BookingDetail> details, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(booking);
        ArgumentNullException.ThrowIfNull(details);
        return _dao.CreateBookingWithTransactionAsync(booking, details, cancellationToken);
    }

    public Task<List<Booking>> GetRecentBookingsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return _dao.GetRecentBookingsAsync(count, cancellationToken);
    }

    public Task<Dictionary<int, decimal>> GetRoomPricesAsync(IEnumerable<int> roomIds, CancellationToken cancellationToken = default)
    {
        return _dao.GetRoomPricesAsync(roomIds, cancellationToken);
    }

    public Task<bool> CancelBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return _dao.CancelBookingAsync(bookingId, cancellationToken);
    }

    public Task<List<int>> GetOverlappingRoomIdsAsync(IEnumerable<int> roomIds, DateTime checkIn, DateTime checkOut, CancellationToken cancellationToken = default)
    {
        return _dao.GetOverlappingRoomIdsAsync(roomIds, checkIn, checkOut, cancellationToken);
    }
}


