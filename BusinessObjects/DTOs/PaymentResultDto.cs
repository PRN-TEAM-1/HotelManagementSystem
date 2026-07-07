namespace BusinessObjects.DTOs;

public sealed class PaymentResultDto
{
    public int PaymentId { get; set; }

    public int InvoiceId { get; set; }

    public int BookingId { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public string InvoiceStatus { get; set; } = string.Empty;

    public string? BookingStatus { get; set; }
}
