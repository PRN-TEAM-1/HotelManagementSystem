using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class Invoice
{
    public int InvoiceId { get; set; }

    public int BookingId { get; set; }

    public int CreatedByUserId { get; set; }

    public decimal RoomAmount { get; set; }

    public decimal ServiceAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public DateTime CreateDate { get; set; }

    public InvoiceStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
