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
            .Where(room => room.Status == RoomOperationalStatus.Available && !occupiedRoomIds.Contains(room.RoomId))
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

        var activeDetails = await context.BookingDetails
            .AsNoTracking()
            .Where(detail => detail.Status != BookingDetailStatus.Cancelled && detail.Status != BookingDetailStatus.CheckedOut)
            .Where(detail => detail.CheckInDate <= currentDate && detail.CheckOutDate >= currentDate)
            .ToListAsync(cancellationToken);

        var rooms = await context.Rooms
            .AsNoTracking()
            .Include(room => room.RoomType)
            .OrderBy(room => room.Floor)
            .ThenBy(room => room.RoomNumber)
            .ToListAsync(cancellationToken);

        var detailsByRoomId = activeDetails
            .GroupBy(d => d.RoomId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var room in rooms)
        {
            if (room.Status == RoomOperationalStatus.Maintenance ||
                room.Status == RoomOperationalStatus.Inactive ||
                room.Status == RoomOperationalStatus.Cleaning)
            {
                continue; // Priority 1, 2, 3 take precedence!
            }

            if (detailsByRoomId.TryGetValue(room.RoomId, out var detail))
            {
                if (detail.Status == BookingDetailStatus.CheckedIn)
                {
                    room.Status = RoomOperationalStatus.Occupied;
                }
                else if (detail.Status == BookingDetailStatus.Reserved)
                {
                    room.Status = RoomOperationalStatus.Reserved;
                }
            }
        }


        return rooms;
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

        if (room.RoomTypeId > 0)
        {
            existingRoom.RoomTypeId = room.RoomTypeId;
        }
        existingRoom.RoomNumber = room.RoomNumber;
        existingRoom.Floor = room.Floor;
        existingRoom.Status = room.Status;
        if (room.Note != null)
        {
            existingRoom.Note = room.Note;
        }
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
