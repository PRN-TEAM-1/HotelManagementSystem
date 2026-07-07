namespace BusinessObjects.DTOs.Dashboard
{
    public class RoomStatusSummaryDto
    {
        public int TotalRooms { get; set; }

        public int AvailableRooms { get; set; }

        public int OccupiedRooms { get; set; }

        public int ReservedRooms { get; set; }

        public int CleaningRooms { get; set; }

        public int MaintenanceRooms { get; set; }
    }
}