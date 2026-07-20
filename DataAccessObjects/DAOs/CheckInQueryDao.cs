using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class CheckInQueryDao
{
    public async Task<List<CheckInCandidateDto>> GetCandidatesForCheckInAsync(CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var candidates = await context.BookingDetails
            .AsNoTracking()
            .Where(bd => bd.Status == BookingDetailStatus.Reserved)
            .Join(
                context.Rooms.AsNoTracking(),
                bd => bd.RoomId,
                r => r.RoomId,
                (bd, r) => new { BookingDetail = bd, Room = r })
            .Join(
                context.RoomTypes.AsNoTracking(),
                x => x.Room.RoomTypeId,
                rt => rt.RoomTypeId,
                (x, rt) => new { x.BookingDetail, x.Room, RoomType = rt })
            .Join(
                context.Bookings.AsNoTracking(),
                x => x.BookingDetail.BookingId,
                b => b.BookingId,
                (x, b) => new CheckInCandidateDto
                {
                    BookingDetailId = x.BookingDetail.BookingDetailId,
                    BookingId = x.BookingDetail.BookingId,
                    RoomId = x.Room.RoomId,
                    RoomNumber = x.Room.RoomNumber,
                    Floor = x.Room.Floor,
                    RoomType = x.RoomType.TypeName,
                    CheckInDate = x.BookingDetail.CheckInDate,
                    CheckOutDate = x.BookingDetail.CheckOutDate,
                    RoomPrice = x.BookingDetail.RoomPrice,
                    NumberOfNights = x.BookingDetail.NumberOfNights,
                    BookingDetailStatus = x.BookingDetail.Status.ToString(),
                    RoomStatus = x.Room.Status.ToString()
                })
            .Where(dto => dto.RoomType != null)
            .OrderBy(dto => dto.BookingId)
            .ThenBy(dto => dto.RoomNumber)
            .ToListAsync(cancellationToken);

        return candidates;
    }

    public async Task<CheckInCandidateDto?> GetCheckInCandidateByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var candidate = await context.BookingDetails
            .AsNoTracking()
            .Where(bd => bd.BookingDetailId == bookingDetailId && bd.Status == BookingDetailStatus.Reserved)
            .Join(
                context.Rooms.AsNoTracking(),
                bd => bd.RoomId,
                r => r.RoomId,
                (bd, r) => new { BookingDetail = bd, Room = r })
            .Join(
                context.RoomTypes.AsNoTracking(),
                x => x.Room.RoomTypeId,
                rt => rt.RoomTypeId,
                (x, rt) => new { x.BookingDetail, x.Room, RoomType = rt })
            .Select(x => new CheckInCandidateDto
            {
                BookingDetailId = x.BookingDetail.BookingDetailId,
                BookingId = x.BookingDetail.BookingId,
                RoomId = x.Room.RoomId,
                RoomNumber = x.Room.RoomNumber,
                Floor = x.Room.Floor,
                RoomType = x.RoomType.TypeName,
                CheckInDate = x.BookingDetail.CheckInDate,
                CheckOutDate = x.BookingDetail.CheckOutDate,
                RoomPrice = x.BookingDetail.RoomPrice,
                NumberOfNights = x.BookingDetail.NumberOfNights,
                BookingDetailStatus = x.BookingDetail.Status.ToString(),
                RoomStatus = x.Room.Status.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return candidate;
    }
}
