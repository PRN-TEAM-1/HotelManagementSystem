namespace BusinessObjects.DTOs;

public sealed class ServiceOrderListItemDto
{
    public int ServiceOrderId { get; set; }

    public int BookingDetailId { get; set; }

    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }
}
