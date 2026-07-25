namespace BusinessObjects.DTOs.Reports;

public sealed class ServiceRevenueDto
{
    public string ServiceName { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public decimal TotalRevenue { get; set; }
}
