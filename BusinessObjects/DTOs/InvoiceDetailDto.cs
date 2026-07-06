namespace BusinessObjects.DTOs;

public sealed class InvoiceDetailDto
{
    public int InvoiceId { get; set; }

    public int BookingId { get; set; }

    public DateTime BookingDate { get; set; }

    public string BookingStatus { get; set; } = string.Empty;

    public int CreatedByUserId { get; set; }

    public string CreatedByUserName { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerIdentityCard { get; set; }

    public decimal RoomAmount { get; set; }

    public decimal ServiceAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public DateTime CreateDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }

    public List<InvoiceRoomLineDto> RoomLines { get; set; } = new();

    public List<InvoiceServiceLineDto> ServiceLines { get; set; } = new();

    public List<InvoicePaymentLineDto> Payments { get; set; } = new();
}
