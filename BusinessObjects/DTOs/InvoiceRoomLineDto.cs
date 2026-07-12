namespace BusinessObjects.DTOs;

public sealed class InvoiceRoomLineDto
{
    public int BookingDetailId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string RoomType { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public int NumberOfNights { get; set; }

    public decimal RoomPrice { get; set; }

    public decimal RoomTotal { get; set; }

    public string Status { get; set; } = string.Empty;
}
