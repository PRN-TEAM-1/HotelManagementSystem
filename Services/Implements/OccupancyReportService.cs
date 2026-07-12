using BusinessObjects.DTOs.Reports;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class OccupancyReportService : IOccupancyReportService
{
    private readonly IOccupancyReportRepository _occupancyReportRepository;

    public OccupancyReportService(IOccupancyReportRepository occupancyReportRepository)
    {
        _occupancyReportRepository = occupancyReportRepository;
    }

    public List<OccupancyReportDto> GetOccupancyReport(ReportFilterDto filter)
    {
        if (filter.EndDate.Date < filter.StartDate.Date)
        {
            return new List<OccupancyReportDto>();
        }

        return _occupancyReportRepository.GetOccupancyReport(filter);
    }
}