namespace BusinessObjects.DTOs;

public sealed class InvoiceSummaryDto
{
    public int InvoiceId { get; set; }

    public int BookingId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime CreateDate { get; set; }

    public decimal RoomAmount { get; set; }

    public decimal ServiceAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public string Status { get; set; } = string.Empty;
}
