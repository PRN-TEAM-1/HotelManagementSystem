using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class RoomDao
{
    public async Task<List<Room>> GetRoomsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var query = context.Rooms
            .AsNoTracking()
            .Include(room => room.RoomType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";
            query = query.Where(room =>
                EF.Functions.Like(room.RoomNumber, pattern)
                || (room.RoomType != null && EF.Functions.Like(room.RoomType.TypeName, pattern))
                || EF.Functions.Like(room.Note ?? string.Empty, pattern));
        }

        return await query
            .OrderBy(room => room.Floor)
            .ThenBy(room => room.RoomNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var occupiedRoomIds = await context.BookingDetails
            .AsNoTracking()
            .Where(detail => detail.Status != BookingDetailStatus.Cancelled && detail.Status != BookingDetailStatus.CheckedOut)
            .Where(detail => detail.CheckInDate < checkOutDate && detail.CheckOutDate > checkInDate)
            .Select(detail => detail.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var query = context.Rooms
            .AsNoTracking()
            .Include(room => room.RoomType)
            .Where(room => !occupiedRoomIds.Contains(room.RoomId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";
            query = query.Where(room =>
                EF.Functions.Like(room.RoomNumber, pattern)
                || (room.RoomType != null && EF.Functions.Like(room.RoomType.TypeName, pattern)));
        }

        return await query
            .OrderBy(room => room.Floor)
            .ThenBy(room => room.RoomNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Room>> GetRoomMapAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var currentDate = asOfDate ?? DateTime.Today;

        var occupiedRoomIds = await context.BookingDetails
            .AsNoTracking()
            .Where(detail => detail.Status != BookingDetailStatus.Cancelled && detail.Status != BookingDetailStatus.CheckedOut)
            .Where(detail => detail.CheckInDate <= currentDate && detail.CheckOutDate >= currentDate)
            .Select(detail => detail.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await context.Rooms
            .AsNoTracking()
            .Include(room => room.RoomType)
            .OrderBy(room => room.Floor)
            .ThenBy(room => room.RoomNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Room> AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(room);

        await using var context = DbContextFactory.CreateDbContext();

        room.CreatedAt = DateTime.Now;
        room.UpdatedAt = DateTime.Now;

        context.Rooms.Add(room);
        await context.SaveChangesAsync(cancellationToken);
        return room;
    }

    public async Task<Room?> UpdateAsync(Room room, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(room);

        await using var context = DbContextFactory.CreateDbContext();

        var existingRoom = await context.Rooms.FirstOrDefaultAsync(item => item.RoomId == room.RoomId, cancellationToken);

        if (existingRoom is null)
        {
            return null;
        }

        existingRoom.RoomTypeId = room.RoomTypeId;
        existingRoom.RoomNumber = room.RoomNumber;
        existingRoom.Floor = room.Floor;
        existingRoom.Status = room.Status;
        existingRoom.Note = room.Note;
        existingRoom.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync(cancellationToken);
        return existingRoom;
    }

    public async Task<bool> RoomNumberExistsAsync(
        string roomNumber,
        int? excludedRoomId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
        {
            return false;
        }

        await using var context = DbContextFactory.CreateDbContext();

        var normalized = roomNumber.Trim();

        return await context.Rooms.AnyAsync(room =>
            room.RoomNumber == normalized && (!excludedRoomId.HasValue || room.RoomId != excludedRoomId.Value), cancellationToken);
    }
}
