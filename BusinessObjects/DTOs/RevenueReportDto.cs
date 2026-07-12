namespace BusinessObjects.DTOs.Reports;

public class RevenueReportDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PaymentCount { get; set; }
}
