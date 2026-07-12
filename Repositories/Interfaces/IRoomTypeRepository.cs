using BusinessObjects.Entities;

namespace Repositories.Interfaces;

public interface IRoomTypeRepository
{
    Task<List<RoomType>> GetRoomTypesAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<RoomType> AddAsync(RoomType roomType, CancellationToken cancellationToken = default);

    Task<RoomType?> UpdateAsync(RoomType roomType, CancellationToken cancellationToken = default);

    Task<bool> TypeNameExistsAsync(
        string typeName,
        int? excludedRoomTypeId = null,
        CancellationToken cancellationToken = default);
}
