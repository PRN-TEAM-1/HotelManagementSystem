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
                PaymentCount = g.Count(),
                AveragePayment = g.Average(x => x.Amount),
                CashRevenue = g.Where(x => x.PaymentMethod == PaymentMethod.Cash).Sum(x => x.Amount),
                BankTransferRevenue = g.Where(x => x.PaymentMethod == PaymentMethod.BankTransfer).Sum(x => x.Amount),
                CardRevenue = g.Where(x => x.PaymentMethod == PaymentMethod.CreditCard).Sum(x => x.Amount),
                EWalletRevenue = g.Where(x => x.PaymentMethod == PaymentMethod.EWallet).Sum(x => x.Amount)
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

    public List<ServiceRevenueDto> GetRevenueByService(ReportFilterDto filter)
    {
        using var context = DbContextFactory.CreateDbContext();

        var fromDate = filter.StartDate.Date;
        var toDate = filter.EndDate.Date.AddDays(1);

        return context.ServiceOrders
            .AsNoTracking()
            .Where(so => so.Status == ServiceOrderStatus.Ordered
                         && so.OrderDate >= fromDate
                         && so.OrderDate < toDate)
            .Join(
                context.Services.AsNoTracking(),
                order => order.ServiceId,
                service => service.ServiceId,
                (order, service) => new
                {
                    service.ServiceName,
                    order.Quantity,
                    order.TotalPrice
                })
            .GroupBy(x => x.ServiceName)
            .Select(g => new ServiceRevenueDto
            {
                ServiceName = g.Key,
                QuantityOrdered = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();
    }
}
