namespace BusinessObjects.DTOs;

public sealed class InvoicePaymentLineDto
{
    public int PaymentId { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? TransactionCode { get; set; }

    public string Status { get; set; } = string.Empty;
}
