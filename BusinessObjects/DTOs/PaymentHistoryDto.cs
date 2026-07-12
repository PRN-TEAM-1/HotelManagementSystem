namespace BusinessObjects.DTOs;

public sealed class PaymentHistoryDto
{
    public int PaymentId { get; set; }

    public int InvoiceId { get; set; }

    public int ReceivedByUserId { get; set; }

    public string ReceivedByUserName { get; set; } = string.Empty;

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? TransactionCode { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }
}
