using BusinessObjects.DTOs.Reports;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class RevenueReportRepository : IRevenueReportRepository
{
    private readonly RevenueReportDao _dao = new();

    public List<RevenueReportDto> GetRevenueReport(ReportFilterDto filter)
    {
        return _dao.GetRevenueReport(filter);
    }

    public List<PaymentRevenueDto> GetRevenueByPaymentMethod(ReportFilterDto filter)
    {
        return _dao.GetRevenueByPaymentMethod(filter);
    }
}