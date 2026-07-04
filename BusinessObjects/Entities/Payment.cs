using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class Payment
{
    public int PaymentId { get; set; }

    public int InvoiceId { get; set; }

    public int ReceivedByUserId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? TransactionCode { get; set; }

    public PaymentStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
