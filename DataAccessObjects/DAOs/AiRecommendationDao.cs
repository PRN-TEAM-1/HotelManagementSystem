using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class AiRecommendationDao
{
    public async Task<AiServiceRecommendationContextDto?> GetServiceRecommendationContextAsync(
        int bookingDetailId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var stay = await (
            from bookingDetail in context.BookingDetails.AsNoTracking()
            join booking in context.Bookings.AsNoTracking()
                on bookingDetail.BookingId equals booking.BookingId
            join customer in context.Customers.AsNoTracking()
                on booking.CustomerId equals customer.CustomerId
            join room in context.Rooms.AsNoTracking()
                on bookingDetail.RoomId equals room.RoomId
            join roomType in context.RoomTypes.AsNoTracking()
                on room.RoomTypeId equals roomType.RoomTypeId
            where bookingDetail.BookingDetailId == bookingDetailId
                  && bookingDetail.Status == BookingDetailStatus.CheckedIn
            select new
            {
                bookingDetail.BookingDetailId,
                bookingDetail.BookingId,
                booking.CustomerId,
                CustomerName = customer.FullName,
                room.RoomNumber,
                RoomType = roomType.TypeName,
                bookingDetail.CheckInDate,
                bookingDetail.CheckOutDate,
                bookingDetail.NumberOfNights,
                bookingDetail.RoomPrice,
                bookingDetail.RoomTotal,
                BookingNote = booking.Note,
                StayNote = bookingDetail.Note
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (stay is null)
        {
            return null;
        }

        var checkRecord = await context.CheckRecords
            .AsNoTracking()
            .Where(record => record.BookingDetailId == bookingDetailId)
            .OrderByDescending(record => record.ActualCheckInDate ?? record.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var activeServices = await context.Services
            .AsNoTracking()
            .Where(service => service.Status == ServiceStatus.Active)
            .OrderBy(service => service.Category)
            .ThenBy(service => service.ServiceName)
            .Select(service => new AiCatalogServiceDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Category = service.Category,
                Price = service.Price
            })
            .ToListAsync(cancellationToken);

        var existingOrders = await (
            from serviceOrder in context.ServiceOrders.AsNoTracking()
            join service in context.Services.AsNoTracking()
                on serviceOrder.ServiceId equals service.ServiceId
            where serviceOrder.BookingDetailId == bookingDetailId
                  && serviceOrder.Status != ServiceOrderStatus.Cancelled
            orderby serviceOrder.OrderDate descending
            select new AiExistingServiceOrderDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Category = service.Category,
                Quantity = serviceOrder.Quantity,
                TotalPrice = serviceOrder.TotalPrice,
                OrderDate = serviceOrder.OrderDate
            })
            .ToListAsync(cancellationToken);

        var historyRows = await (
            from booking in context.Bookings.AsNoTracking()
            join bookingDetail in context.BookingDetails.AsNoTracking()
                on booking.BookingId equals bookingDetail.BookingId
            join serviceOrder in context.ServiceOrders.AsNoTracking()
                on bookingDetail.BookingDetailId equals serviceOrder.BookingDetailId
            join service in context.Services.AsNoTracking()
                on serviceOrder.ServiceId equals service.ServiceId
            where booking.CustomerId == stay.CustomerId
                  && serviceOrder.Status != ServiceOrderStatus.Cancelled
            select new
            {
                service.ServiceId,
                service.ServiceName,
                service.Category,
                serviceOrder.Quantity,
                serviceOrder.OrderDate
            })
            .ToListAsync(cancellationToken);

        var guestHistory = historyRows
            .GroupBy(row => new { row.ServiceId, row.ServiceName, row.Category })
            .Select(group => new AiGuestServiceHistoryDto
            {
                ServiceId = group.Key.ServiceId,
                ServiceName = group.Key.ServiceName,
                Category = group.Key.Category,
                TimesOrdered = group.Count(),
                TotalQuantity = group.Sum(row => row.Quantity),
                LastOrderedAt = group.Max(row => row.OrderDate)
            })
            .OrderByDescending(row => row.LastOrderedAt)
            .Take(8)
            .ToList();

        var guestStayCount = await context.Bookings
            .AsNoTracking()
            .CountAsync(booking => booking.CustomerId == stay.CustomerId, cancellationToken);

        return new AiServiceRecommendationContextDto
        {
            BookingDetailId = stay.BookingDetailId,
            BookingId = stay.BookingId,
            CustomerId = stay.CustomerId,
            CustomerName = stay.CustomerName,
            RoomNumber = stay.RoomNumber,
            RoomType = stay.RoomType,
            CheckInDate = stay.CheckInDate,
            CheckOutDate = stay.CheckOutDate,
            ActualCheckInDate = checkRecord?.ActualCheckInDate,
            NumberOfNights = stay.NumberOfNights,
            RoomPrice = stay.RoomPrice,
            RoomTotal = stay.RoomTotal,
            BookingNote = stay.BookingNote ?? string.Empty,
            StayNote = stay.StayNote ?? string.Empty,
            CheckInNote = checkRecord?.CheckInNote ?? string.Empty,
            GuestStayCount = guestStayCount,
            CurrentServiceTotal = existingOrders.Sum(order => order.TotalPrice),
            ActiveServices = activeServices,
            ExistingServiceOrders = existingOrders,
            GuestServiceHistory = guestHistory
        };
    }
}
