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
                        c.ActualCheckInDate.Value.Date == today),

                TodayCheckOuts = context.CheckRecords
                    .AsNoTracking()
                    .Count(c =>
                        c.ActualCheckOutDate.HasValue &&
                        c.ActualCheckOutDate.Value.Date == today)
            }
        };

        return summary;
    }
}