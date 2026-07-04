using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class Room
{
    public int RoomId { get; set; }

    public int RoomTypeId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Floor { get; set; }

    public RoomOperationalStatus Status { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public RoomType? RoomType { get; set; }
}
