namespace BusinessObjects.DTOs;

public sealed class CheckInCandidateDto
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Floor { get; set; }

    public string RoomType { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal RoomPrice { get; set; }

    public int NumberOfNights { get; set; }

    public string BookingDetailStatus { get; set; } = string.Empty;

    public string RoomStatus { get; set; } = string.Empty;
}
