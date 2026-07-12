namespace BusinessObjects.DTOs;

public sealed class RoomTypeListItemDto
{
    public int RoomTypeId { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int Capacity { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class CreateRoomTypeRequestDto
{
    public string TypeName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int Capacity { get; set; }

    public string Status { get; set; } = string.Empty;
}

public sealed class UpdateRoomTypeRequestDto : CreateRoomTypeRequestDto
{
    public int RoomTypeId { get; set; }
}
