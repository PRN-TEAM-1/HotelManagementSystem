namespace BusinessObjects.DTOs;

public sealed class CheckoutCandidateDto
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Floor { get; set; }

    public string RoomType { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public DateTime? ActualCheckInDate { get; set; }

    public string BookingDetailStatus { get; set; } = string.Empty;

    public int ServiceOrderCount { get; set; }

    public decimal ServiceOrderTotal { get; set; }
}
