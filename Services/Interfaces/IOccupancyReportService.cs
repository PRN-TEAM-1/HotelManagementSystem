using System;
using System.Collections.Generic;
using System.Text;
using BusinessObjects.DTOs.Reports;

namespace Services.Interfaces
{
    public interface IOccupancyReportService
    {
        List<OccupancyReportDto> GetOccupancyReport(ReportFilterDto filter);
    }
}
