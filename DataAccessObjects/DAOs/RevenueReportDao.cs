using BusinessObjects.DTOs.Reports;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Enums;
namespace DataAccessObjects;

public sealed class RevenueReportDao
{
    public List<RevenueReportDto> GetRevenueReport(ReportFilterDto filter)
    {
        using var context = DbContextFactory.CreateDbContext();

        var fromDate = filter.StartDate.Date;
        var toDate = filter.EndDate.Date.AddDays(1);

        return context.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Success
                        && p.PaymentDate >= fromDate
                        && p.PaymentDate < toDate)
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new RevenueReportDto
            {
                Date = g.Key,
                TotalRevenue = g.Sum(x => x.Amount),
                PaymentCount = g.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();
    }

    public List<PaymentRevenueDto> GetRevenueByPaymentMethod(ReportFilterDto filter)
    {
        using var context = DbContextFactory.CreateDbContext();

        var fromDate = filter.StartDate.Date;
        var toDate = filter.EndDate.Date.AddDays(1);

        return context.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Success
                        && p.PaymentDate >= fromDate
                        && p.PaymentDate < toDate)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentRevenueDto
            {
                PaymentMethod = g.Key.ToString(),
                TotalRevenue = g.Sum(x => x.Amount),
                PaymentCount = g.Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();
    }
}