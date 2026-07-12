using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.DTOs.Reports
{
    public sealed class ServiceUsageReportDto
    {
        public string ServiceName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public int QuantityOrdered { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}
