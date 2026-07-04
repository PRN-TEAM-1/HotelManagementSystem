namespace BusinessObjects.DTOs;

public sealed class CheckoutResultDto
{
    public int BookingDetailId { get; set; }

    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public DateTime ActualCheckOutDate { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }
}
