namespace BusinessObjects.DTOs;

public sealed class CreateInvoiceRequestDto
{
    public int BookingId { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public string? Note { get; set; }
}
