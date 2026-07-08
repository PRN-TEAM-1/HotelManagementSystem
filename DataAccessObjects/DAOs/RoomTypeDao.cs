using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class RoomTypeDao
{
    public async Task<List<RoomType>> GetRoomTypesAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var query = context.RoomTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";
            query = query.Where(roomType =>
                EF.Functions.Like(roomType.TypeName, pattern)
                || (roomType.Description != null && EF.Functions.Like(roomType.Description, pattern)));
        }

        return await query
            .OrderBy(roomType => roomType.TypeName)
            .ToListAsync(cancellationToken);
    }

    public async Task<RoomType> AddAsync(RoomType roomType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roomType);

        await using var context = DbContextFactory.CreateDbContext();

        roomType.CreatedAt = DateTime.Now;
        roomType.UpdatedAt = DateTime.Now;

        context.RoomTypes.Add(roomType);
        await context.SaveChangesAsync(cancellationToken);
        return roomType;
    }

    public async Task<RoomType?> UpdateAsync(RoomType roomType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roomType);

        await using var context = DbContextFactory.CreateDbContext();

        var existing = await context.RoomTypes.FirstOrDefaultAsync(item => item.RoomTypeId == roomType.RoomTypeId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.TypeName = roomType.TypeName;
        existing.Description = roomType.Description;
        existing.BasePrice = roomType.BasePrice;
        existing.Capacity = roomType.Capacity;
        existing.Status = roomType.Status;
        existing.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> TypeNameExistsAsync(
        string typeName,
        int? excludedRoomTypeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return false;
        }

        await using var context = DbContextFactory.CreateDbContext();
        var normalized = typeName.Trim();

        return await context.RoomTypes.AnyAsync(roomType =>
            roomType.TypeName == normalized && (!excludedRoomTypeId.HasValue || roomType.RoomTypeId != excludedRoomTypeId.Value), cancellationToken);
    }
}
