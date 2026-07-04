using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class BookingDetail
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal RoomPrice { get; set; }

    public int NumberOfNights { get; set; }

    public decimal RoomTotal { get; set; }

    public BookingDetailStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
