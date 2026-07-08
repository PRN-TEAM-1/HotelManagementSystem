using BusinessObjects.DTOs.Dashboard;
public class DashboardSummaryDto
{
    public RoomStatusSummaryDto RoomSummary { get; set; } = new();

    public BookingStatusSummaryDto BookingSummary { get; set; } = new();

    public TodayOperationSummaryDto TodayOperation { get; set; } = new();
}