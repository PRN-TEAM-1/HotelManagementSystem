using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.DTOs.Reports
{
    public sealed class ReportFilterDto
    {
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);

        public DateTime EndDate { get; set; } = DateTime.Today;
    }
}
