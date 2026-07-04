using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class Booking
{
    public int BookingId { get; set; }

    public int CustomerId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime BookingDate { get; set; }

    public BookingStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
