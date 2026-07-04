using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class BookingOperationDao
{
    public async Task<BookingDetail?> GetBookingDetailByIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.BookingDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(bd => bd.BookingDetailId == bookingDetailId, cancellationToken);
    }

    public async Task<Room?> GetRoomByIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);
    }

    public async Task UpdateBookingDetailStatusAsync(int bookingDetailId, BookingDetailStatus status, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var bookingDetail = await context.BookingDetails.FindAsync(new object?[] { bookingDetailId }, cancellationToken);

        if (bookingDetail is not null)
        {
            bookingDetail.Status = status;
            bookingDetail.UpdatedAt = DateTime.Now;

            context.BookingDetails.Update(bookingDetail);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateRoomStatusAsync(int roomId, RoomOperationalStatus status, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var room = await context.Rooms.FindAsync(new object?[] { roomId }, cancellationToken);

        if (room is not null)
        {
            room.Status = status;
            room.UpdatedAt = DateTime.Now;

            context.Rooms.Update(room);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsRoomOperationalAsync(int roomId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var room = await context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);

        if (room is null)
        {
            return false;
        }

        return room.Status != RoomOperationalStatus.Maintenance &&
               room.Status != RoomOperationalStatus.Cleaning &&
               room.Status != RoomOperationalStatus.Inactive;
    }
}
