using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.DTOs.Reports

{
    public sealed class OccupancyReportDto
    {
        public string RoomNumber { get; set; } = string.Empty;

        public string RoomType { get; set; } = string.Empty;

        public int TotalNightsBooked { get; set; }

        public double OccupancyRate { get; set; }
    }
}
