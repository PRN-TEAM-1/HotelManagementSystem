using BusinessObjects.DTOs.Reports;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class ServiceUsageReportRepository : IServiceUsageReportRepository
{
    private readonly ServiceUsageReportDao _dao = new();

    public List<ServiceUsageReportDto> GetServiceUsageReport(ReportFilterDto filter)
    {
        return _dao.GetServiceUsageReport(filter);
    }
}