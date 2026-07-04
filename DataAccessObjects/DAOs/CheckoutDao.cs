using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class CheckoutDao
{
    public async Task<List<CheckoutCandidateDto>> GetCandidatesForCheckoutAsync(CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var candidates = await context.BookingDetails
            .AsNoTracking()
            .Where(bd => bd.Status == BookingDetailStatus.CheckedIn)
            .Join(
                context.CheckRecords.AsNoTracking(),
                bd => bd.BookingDetailId,
                cr => cr.BookingDetailId,
                (bd, cr) => new { BookingDetail = bd, CheckRecord = cr })
            .Join(
                context.Rooms.AsNoTracking(),
                x => x.BookingDetail.RoomId,
                r => r.RoomId,
                (x, r) => new { x.BookingDetail, x.CheckRecord, Room = r })
            .Join(
                context.RoomTypes.AsNoTracking(),
                x => x.Room.RoomTypeId,
                rt => rt.RoomTypeId,
                (x, rt) => new { x.BookingDetail, x.CheckRecord, x.Room, RoomType = rt })
            .Join(
                context.Bookings.AsNoTracking(),
                x => x.BookingDetail.BookingId,
                b => b.BookingId,
                (x, b) => new CheckoutCandidateDto
                {
                    BookingDetailId = x.BookingDetail.BookingDetailId,
                    BookingId = x.BookingDetail.BookingId,
                    RoomId = x.Room.RoomId,
                    RoomNumber = x.Room.RoomNumber,
                    Floor = x.Room.Floor,
                    RoomType = x.RoomType.TypeName,
                    CheckInDate = x.BookingDetail.CheckInDate,
                    CheckOutDate = x.BookingDetail.CheckOutDate,
                    ActualCheckInDate = x.CheckRecord.ActualCheckInDate,
                    BookingDetailStatus = x.BookingDetail.Status.ToString(),
                    ServiceOrderCount = 0,
                    ServiceOrderTotal = 0
                })
            .OrderBy(dto => dto.BookingId)
            .ThenBy(dto => dto.RoomNumber)
            .ToListAsync(cancellationToken);

        // Populate service order count and total for each candidate
        foreach (var candidate in candidates)
        {
            candidate.ServiceOrderCount = await context.ServiceOrders
                .AsNoTracking()
                .Where(so => so.BookingDetailId == candidate.BookingDetailId && so.Status != ServiceOrderStatus.Cancelled)
                .CountAsync(cancellationToken);

            candidate.ServiceOrderTotal = await context.ServiceOrders
                .AsNoTracking()
                .Where(so => so.BookingDetailId == candidate.BookingDetailId && so.Status != ServiceOrderStatus.Cancelled)
                .SumAsync(so => so.TotalPrice, cancellationToken);
        }

        return candidates;
    }

    public async Task<CheckoutCandidateDto?> GetCheckoutCandidateByBookingDetailIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var candidate = await context.BookingDetails
            .AsNoTracking()
            .Where(bd => bd.BookingDetailId == bookingDetailId && bd.Status == BookingDetailStatus.CheckedIn)
            .Join(
                context.CheckRecords.AsNoTracking(),
                bd => bd.BookingDetailId,
                cr => cr.BookingDetailId,
                (bd, cr) => new { BookingDetail = bd, CheckRecord = cr })
            .Join(
                context.Rooms.AsNoTracking(),
                x => x.BookingDetail.RoomId,
                r => r.RoomId,
                (x, r) => new { x.BookingDetail, x.CheckRecord, Room = r })
            .Join(
                context.RoomTypes.AsNoTracking(),
                x => x.Room.RoomTypeId,
                rt => rt.RoomTypeId,
                (x, rt) => new { x.BookingDetail, x.CheckRecord, x.Room, RoomType = rt })
            .Select(x => new CheckoutCandidateDto
            {
                BookingDetailId = x.BookingDetail.BookingDetailId,
                BookingId = x.BookingDetail.BookingId,
                RoomId = x.Room.RoomId,
                RoomNumber = x.Room.RoomNumber,
                Floor = x.Room.Floor,
                RoomType = x.RoomType.TypeName,
                CheckInDate = x.BookingDetail.CheckInDate,
                CheckOutDate = x.BookingDetail.CheckOutDate,
                ActualCheckInDate = x.CheckRecord.ActualCheckInDate,
                BookingDetailStatus = x.BookingDetail.Status.ToString(),
                ServiceOrderCount = 0,
                ServiceOrderTotal = 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (candidate is not null)
        {
            candidate.ServiceOrderCount = await context.ServiceOrders
                .AsNoTracking()
                .Where(so => so.BookingDetailId == bookingDetailId && so.Status != ServiceOrderStatus.Cancelled)
                .CountAsync(cancellationToken);

            candidate.ServiceOrderTotal = await context.ServiceOrders
                .AsNoTracking()
                .Where(so => so.BookingDetailId == bookingDetailId && so.Status != ServiceOrderStatus.Cancelled)
                .SumAsync(so => so.TotalPrice, cancellationToken);
        }

        return candidate;
    }
}
