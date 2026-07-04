namespace BusinessObjects.DTOs;

public sealed class CheckInRequestDto
{
    public int BookingDetailId { get; set; }

    public string? CheckInNote { get; set; }
}
