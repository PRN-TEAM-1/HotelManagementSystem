namespace BusinessObjects.DTOs;

public sealed class InvoiceCandidateDto
{
    public int BookingId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public string BookingStatus { get; set; } = string.Empty;

    public int RoomCount { get; set; }

    public decimal RoomAmount { get; set; }

    public decimal ServiceAmount { get; set; }

    public decimal EstimatedTotalAmount { get; set; }
}
