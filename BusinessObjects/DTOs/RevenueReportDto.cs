namespace BusinessObjects.DTOs.Reports;

public class RevenueReportDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PaymentCount { get; set; }
    public decimal AveragePayment { get; set; }
    public decimal CashRevenue { get; set; }
    public decimal BankTransferRevenue { get; set; }
    public decimal CardRevenue { get; set; }
    public decimal EWalletRevenue { get; set; }
}
