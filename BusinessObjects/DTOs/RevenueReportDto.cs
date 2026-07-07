using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.DTOs.Reports
{
    public sealed class RevenueReportDto
    {
        public DateTime Date { get; set; }

        public decimal RoomRevenue { get; set; }

        public decimal ServiceRevenue { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
