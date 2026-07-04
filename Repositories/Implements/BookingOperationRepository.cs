using BusinessObjects.Entities;
using BusinessObjects.Enums;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class BookingOperationRepository : IBookingOperationRepository
{
    private readonly BookingOperationDao _dao;

    public BookingOperationRepository(BookingOperationDao? dao = null)
    {
        _dao = dao ?? new BookingOperationDao();
    }

    public async Task<BookingDetail?> GetBookingDetailByIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetBookingDetailByIdAsync(bookingDetailId, cancellationToken);
    }

    public async Task<Room?> GetRoomByIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _dao.GetRoomByIdAsync(roomId, cancellationToken);
    }

    public async Task UpdateBookingDetailStatusAsync(int bookingDetailId, BookingDetailStatus status, CancellationToken cancellationToken = default)
    {
        await _dao.UpdateBookingDetailStatusAsync(bookingDetailId, status, cancellationToken);
    }

    public async Task UpdateRoomStatusAsync(int roomId, RoomOperationalStatus status, CancellationToken cancellationToken = default)
    {
        await _dao.UpdateRoomStatusAsync(roomId, status, cancellationToken);
    }

    public async Task<bool> IsRoomOperationalAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _dao.IsRoomOperationalAsync(roomId, cancellationToken);
    }
}
