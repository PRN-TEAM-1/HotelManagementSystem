namespace BusinessObjects.DTOs;

public sealed class ServiceOrderRequestDto
{
    public int BookingDetailId { get; set; }

    public int ServiceId { get; set; }

    public int Quantity { get; set; }

    public string? Note { get; set; }
}
