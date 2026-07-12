using BusinessObjects.DTOs.Reports;

namespace Repositories.Interfaces;

public interface IRevenueReportRepository
{
    List<RevenueReportDto> GetRevenueReport(ReportFilterDto filter);
    List<PaymentRevenueDto> GetRevenueByPaymentMethod(ReportFilterDto filter);
}