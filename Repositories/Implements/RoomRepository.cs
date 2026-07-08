using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class RoomRepository : IRoomRepository
{
    private readonly RoomDao _dao;

    public RoomRepository(RoomDao? dao = null)
    {
        _dao = dao ?? new RoomDao();
    }

    public Task<List<Room>> GetRoomsAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        return _dao.GetRoomsAsync(searchTerm, cancellationToken);
    }

    public Task<List<Room>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        return _dao.GetAvailableRoomsAsync(checkInDate, checkOutDate, searchTerm, cancellationToken);
    }

    public Task<List<Room>> GetRoomMapAsync(DateTime? asOfDate = null, CancellationToken cancellationToken = default)
    {
        return _dao.GetRoomMapAsync(asOfDate, cancellationToken);
    }

    public Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(room);
        return _dao.AddAsync(room, cancellationToken);
    }

    public Task<Room?> UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(room);
        return _dao.UpdateAsync(room, cancellationToken);
    }

    public Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludedRoomId = null, CancellationToken cancellationToken = default)
    {
        return _dao.RoomNumberExistsAsync(roomNumber, excludedRoomId, cancellationToken);
    }
}
