namespace BusinessObjects.DTOs;

public sealed class ServiceOrderSummaryDto
{
    public int BookingDetailId { get; set; }

    public decimal TotalServiceAmount { get; set; }

    public int ServiceOrderCount { get; set; }

    public List<ServiceOrderListItemDto> ServiceOrders { get; set; } = new();
}
