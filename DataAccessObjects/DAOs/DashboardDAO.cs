using BusinessObjects.DTOs.Dashboard;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class DashboardDao
{
    public DashboardSummaryDto GetDashboardSummary()
    {
        using var context = DbContextFactory.CreateDbContext();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var summary = new DashboardSummaryDto
        {
            RoomSummary = new RoomStatusSummaryDto
            {
                TotalRooms = context.Rooms
                    .AsNoTracking()
                    .Count(),

                AvailableRooms = context.Rooms
                    .AsNoTracking()
                    .Count(r => r.Status == RoomOperationalStatus.Available),

                OccupiedRooms = context.BookingDetails
                    .AsNoTracking()
                    .Count(b => b.Status == BookingDetailStatus.CheckedIn),

                ReservedRooms = context.BookingDetails
                    .AsNoTracking()
                    .Count(b => b.Status == BookingDetailStatus.Reserved),

                CleaningRooms = context.Rooms
                    .AsNoTracking()
                    .Count(r => r.Status == RoomOperationalStatus.Cleaning),

                MaintenanceRooms = context.Rooms
                    .AsNoTracking()
                    .Count(r => r.Status == RoomOperationalStatus.Maintenance)
            },

            BookingSummary = new BookingStatusSummaryDto
            {
                TodayBookings = context.Bookings
                    .AsNoTracking()
                    .Count(b => b.BookingDate.Date == today)
            },

            TodayOperation = new TodayOperationSummaryDto
            {
                TodayCheckIns = context.CheckRecords
                    .AsNoTracking()
                    .Count(c =>
                        c.ActualCheckInDate.HasValue &&
                        c.ActualCheckInDate.Value >= today &&
                        c.ActualCheckInDate.Value < tomorrow),

                TodayCheckOuts = context.CheckRecords
                    .AsNoTracking()
                    .Count(c =>
                        c.ActualCheckOutDate.HasValue &&
                        c.ActualCheckOutDate.Value >= today &&
                        c.ActualCheckOutDate.Value < tomorrow),

                ActiveStays = context.BookingDetails
                    .AsNoTracking()
                    .Count(bd => bd.Status == BookingDetailStatus.CheckedIn),

                DueArrivals = context.BookingDetails
                    .AsNoTracking()
                    .Count(bd => bd.Status == BookingDetailStatus.Reserved
                                 && bd.CheckInDate >= today
                                 && bd.CheckInDate < tomorrow),

                DueDepartures = context.BookingDetails
                    .AsNoTracking()
                    .Count(bd => bd.Status == BookingDetailStatus.CheckedIn
                                 && bd.CheckOutDate >= today
                                 && bd.CheckOutDate < tomorrow)
            }
        };

        return summary;
    }
}
