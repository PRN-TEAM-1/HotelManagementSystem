using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class ServiceOrder
{
    public int ServiceOrderId { get; set; }

    public int BookingDetailId { get; set; }

    public int ServiceId { get; set; }

    public int CreatedByUserId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime OrderDate { get; set; }

    public ServiceOrderStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
