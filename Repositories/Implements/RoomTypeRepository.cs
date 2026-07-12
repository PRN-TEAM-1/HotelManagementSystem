using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class RoomTypeRepository : IRoomTypeRepository
{
    private readonly RoomTypeDao _dao;

    public RoomTypeRepository(RoomTypeDao? dao = null)
    {
        _dao = dao ?? new RoomTypeDao();
    }

    public Task<List<RoomType>> GetRoomTypesAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        return _dao.GetRoomTypesAsync(searchTerm, cancellationToken);
    }

    public Task<RoomType> AddAsync(RoomType roomType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roomType);
        return _dao.AddAsync(roomType, cancellationToken);
    }

    public Task<RoomType?> UpdateAsync(RoomType roomType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roomType);
        return _dao.UpdateAsync(roomType, cancellationToken);
    }

    public Task<bool> TypeNameExistsAsync(string typeName, int? excludedRoomTypeId = null, CancellationToken cancellationToken = default)
    {
        return _dao.TypeNameExistsAsync(typeName, excludedRoomTypeId, cancellationToken);
    }
}
