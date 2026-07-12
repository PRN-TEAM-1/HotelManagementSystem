namespace BusinessObjects.DTOs.Reports;

public class PaymentRevenueDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int PaymentCount { get; set; }
}