using BusinessObjects.DTOs.Reports;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class OccupancyReportRepository : IOccupancyReportRepository
{
    private readonly OccupancyReportDao _occupancyReportDao;

    public OccupancyReportRepository()
    {
        _occupancyReportDao = new OccupancyReportDao();
    }

    public List<OccupancyReportDto> GetOccupancyReport(ReportFilterDto filter)
    {
        return _occupancyReportDao.GetOccupancyReport(filter);
    }
}