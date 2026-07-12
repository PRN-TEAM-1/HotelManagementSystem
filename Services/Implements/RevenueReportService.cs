using BusinessObjects.DTOs.Reports;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class RevenueReportService : IRevenueReportService
{
    private readonly IRevenueReportRepository _repository;

    public RevenueReportService(IRevenueReportRepository repository)
    {
        _repository = repository;
    }

    public List<RevenueReportDto> GetRevenueReport(ReportFilterDto filter)
    {
        if (filter.EndDate < filter.StartDate)
        {
            return new List<RevenueReportDto>();
        }

        return _repository.GetRevenueReport(filter);
    }

    public List<PaymentRevenueDto> GetRevenueByPaymentMethod(ReportFilterDto filter)
    {
        if (filter.EndDate.Date < filter.StartDate.Date)
        {
            return new List<PaymentRevenueDto>();
        }

        return _repository.GetRevenueByPaymentMethod(filter);
    }
}