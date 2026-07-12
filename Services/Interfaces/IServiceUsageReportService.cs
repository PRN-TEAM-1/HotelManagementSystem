using BusinessObjects.DTOs.Reports;

namespace Services.Interfaces;

public interface IServiceUsageReportService
{
    List<ServiceUsageReportDto> GetServiceUsageReport(ReportFilterDto filter);
}