using BusinessObjects.DTOs.Reports;

namespace Services.Interfaces;

public interface IRevenueReportService
{
    List<RevenueReportDto> GetRevenueReport(ReportFilterDto filter);
    List<PaymentRevenueDto> GetRevenueByPaymentMethod(ReportFilterDto filter);
}