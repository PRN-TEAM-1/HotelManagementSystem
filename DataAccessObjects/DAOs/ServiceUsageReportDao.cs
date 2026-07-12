using BusinessObjects.DTOs.Reports;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public sealed class ServiceUsageReportDao
{
    public List<ServiceUsageReportDto> GetServiceUsageReport(ReportFilterDto filter)
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
                serviceOrder => serviceOrder.ServiceId,
                service => service.ServiceId,
                (serviceOrder, service) => new
                {
                    service.ServiceName,
                    serviceOrder.Quantity,
                    serviceOrder.TotalPrice
                })
            .GroupBy(x => x.ServiceName)
            .Select(g => new ServiceUsageReportDto
            {
                ServiceName = g.Key,
                QuantityOrdered = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();
    }
}