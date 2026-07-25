namespace BusinessObjects.DTOs;

public sealed class CreateInvoiceRequestDto
{
    public int BookingId { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal TaxPercent { get; set; }

    public string? Note { get; set; }
}
