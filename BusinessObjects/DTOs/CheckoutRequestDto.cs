namespace BusinessObjects.DTOs;

public sealed class CheckoutRequestDto
{
    public int BookingDetailId { get; set; }

    public string? CheckOutNote { get; set; }
}
