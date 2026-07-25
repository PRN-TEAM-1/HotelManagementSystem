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

    public async Task<CheckRecord> CheckInWithTransactionAsync(
        CheckRecord checkRecord,
        BookingDetailStatus newDetailStatus,
        RoomOperationalStatus newRoomStatus,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Insert CheckRecord
            context.CheckRecords.Add(checkRecord);
            await context.SaveChangesAsync(cancellationToken);

            // Update BookingDetail
            var bookingDetail = await context.BookingDetails.FindAsync(new object?[] { checkRecord.BookingDetailId }, cancellationToken);
            if (bookingDetail != null)
            {
                bookingDetail.Status = newDetailStatus;
                bookingDetail.UpdatedAt = DateTime.Now;
                context.BookingDetails.Update(bookingDetail);
            }

            // Update Room
            if (bookingDetail != null)
            {
                var room = await context.Rooms.FindAsync(new object?[] { bookingDetail.RoomId }, cancellationToken);
                if (room != null)
                {
                    room.Status = newRoomStatus;
                    room.UpdatedAt = DateTime.Now;
                    context.Rooms.Update(room);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return checkRecord;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<CheckRecord> CheckoutWithTransactionAsync(
        CheckRecord checkRecord,
        BookingDetailStatus newDetailStatus,
        RoomOperationalStatus newRoomStatus,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Update CheckRecord
            context.CheckRecords.Update(checkRecord);

            // Update BookingDetail
            var bookingDetail = await context.BookingDetails.FindAsync(new object?[] { checkRecord.BookingDetailId }, cancellationToken);
            if (bookingDetail != null)
            {
                bookingDetail.Status = newDetailStatus;
                bookingDetail.UpdatedAt = DateTime.Now;
                context.BookingDetails.Update(bookingDetail);
            }

            // Update Room
            if (bookingDetail != null)
            {
                var room = await context.Rooms.FindAsync(new object?[] { bookingDetail.RoomId }, cancellationToken);
                if (room != null)
                {
                    room.Status = newRoomStatus;
                    room.UpdatedAt = DateTime.Now;
                    context.Rooms.Update(room);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return checkRecord;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
