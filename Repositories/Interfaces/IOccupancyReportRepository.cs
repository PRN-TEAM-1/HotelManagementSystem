using BusinessObjects.DTOs.Reports;

namespace Repositories.Interfaces;

public interface IOccupancyReportRepository
{
    List<OccupancyReportDto> GetOccupancyReport(ReportFilterDto filter);
}