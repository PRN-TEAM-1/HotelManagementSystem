using BusinessObjects.DTOs.Reports;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class OccupancyReportDao
{
    public List<OccupancyReportDto> GetOccupancyReport(ReportFilterDto filter)
    {
        using var context = DbContextFactory.CreateDbContext();

        var startDate = filter.StartDate.Date;
        var endDate = filter.EndDate.Date;

        if (endDate < startDate)
        {
            return new List<OccupancyReportDto>();
        }

        int totalDays = (endDate - startDate).Days + 1;

        var validStatuses = new[]
        {
            BookingDetailStatus.Reserved,
            BookingDetailStatus.CheckedIn,
            BookingDetailStatus.CheckedOut
        };

        var query =
            from bd in context.BookingDetails.AsNoTracking()
            join room in context.Rooms.AsNoTracking()
                on bd.RoomId equals room.RoomId
            join roomType in context.RoomTypes.AsNoTracking()
                on room.RoomTypeId equals roomType.RoomTypeId
            where validStatuses.Contains(bd.Status)
                  && bd.CheckInDate.Date < endDate.AddDays(1)
                  && bd.CheckOutDate.Date > startDate
            select new
            {
                room.RoomNumber,
                RoomType = roomType.TypeName,
                bd.CheckInDate,
                bd.CheckOutDate
            };

        var rawData = query.ToList();

        return rawData
            .GroupBy(x => new
            {
                x.RoomNumber,
                x.RoomType
            })
            .Select(g =>
            {
                int totalNights = g.Sum(x =>
                {
                    var actualStart = x.CheckInDate.Date < startDate
                        ? startDate
                        : x.CheckInDate.Date;

                    var actualEnd = x.CheckOutDate.Date > endDate.AddDays(1)
                        ? endDate.AddDays(1)
                        : x.CheckOutDate.Date;

                    var nights = (actualEnd - actualStart).Days;
                    return nights < 0 ? 0 : nights;
                });

                return new OccupancyReportDto
                {
                    RoomNumber = g.Key.RoomNumber,
                    RoomType = g.Key.RoomType,
                    TotalNightsBooked = totalNights,
                    OccupancyRate = totalDays == 0
                        ? 0
                        : Math.Round((double)totalNights / totalDays * 100, 2)
                };
            })
            .OrderBy(x => x.RoomNumber)
            .ToList();
    }
}