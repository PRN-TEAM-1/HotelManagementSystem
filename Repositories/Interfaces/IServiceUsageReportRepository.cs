using System;
using System.Collections.Generic;
using System.Text;
using BusinessObjects.DTOs.Reports;

namespace Repositories.Interfaces
{
    public interface IServiceUsageReportRepository
    {
        List<ServiceUsageReportDto> GetServiceUsageReport(ReportFilterDto filter);
    }
}
