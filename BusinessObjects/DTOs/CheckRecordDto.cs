namespace BusinessObjects.DTOs;

public sealed class CheckRecordDto
{
    public int CheckRecordId { get; set; }

    public int BookingDetailId { get; set; }

    public int? CheckInByUserId { get; set; }

    public int? CheckOutByUserId { get; set; }

    public DateTime? ActualCheckInDate { get; set; }

    public DateTime? ActualCheckOutDate { get; set; }

    public string? CheckInNote { get; set; }

    public string? CheckOutNote { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
