using BusinessObjects.DTOs.Reports;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class ServiceUsageReportService : IServiceUsageReportService
{
    private readonly IServiceUsageReportRepository _repository;

    public ServiceUsageReportService(IServiceUsageReportRepository repository)
    {
        _repository = repository;
    }

    public List<ServiceUsageReportDto> GetServiceUsageReport(ReportFilterDto filter)
    {
        if (filter.EndDate.Date < filter.StartDate.Date)
        {
            return new List<ServiceUsageReportDto>();
        }

        return _repository.GetServiceUsageReport(filter);
    }
}