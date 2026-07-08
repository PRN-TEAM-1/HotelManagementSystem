using BusinessObjects.Entities;

namespace Repositories.Interfaces;

public interface IRoomRepository
{
    Task<List<Room>> GetRoomsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<List<Room>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<List<Room>> GetRoomMapAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default);

    Task<Room?> UpdateAsync(Room room, CancellationToken cancellationToken = default);

    Task<bool> RoomNumberExistsAsync(
        string roomNumber,
        int? excludedRoomId = null,
        CancellationToken cancellationToken = default);
}
