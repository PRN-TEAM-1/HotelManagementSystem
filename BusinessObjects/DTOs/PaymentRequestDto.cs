using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public sealed class PaymentRequestDto
{
    public int InvoiceId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? TransactionCode { get; set; }

    public string? Note { get; set; }
}
