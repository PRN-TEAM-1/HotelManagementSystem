namespace BusinessObjects.DTOs;

public sealed class CustomerListItemDto
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? IdentityCard { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class CreateCustomerRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string? IdentityCard { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }
}

public sealed class UpdateCustomerRequestDto : CreateCustomerRequestDto
{
    public int CustomerId { get; set; }
}

public sealed class RoomListItemDto
{
    public int RoomId { get; set; }

    public int RoomTypeId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Floor { get; set; }

    public string Status { get; set; } = string.Empty;

    public string RoomTypeName { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public string? Note { get; set; }
}

public sealed class CreateRoomRequestDto
{
    public int RoomTypeId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public int Floor { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }
}

public sealed class UpdateRoomRequestDto : CreateRoomRequestDto
{
    public int RoomId { get; set; }
}

public sealed class RoomMapItemDto
{
    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = string.Empty;

    public string RoomTypeName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsOccupied { get; set; }

    public string? OccupancyLabel { get; set; }

    public DateTime? CheckInDate { get; set; }

    public DateTime? CheckOutDate { get; set; }
}

public sealed class CreateBookingRequestDto
{
    public int CustomerId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public List<int> RoomIds { get; set; } = new();

    public string? Note { get; set; }
}

public sealed class BookingSummaryDto
{
    public int BookingId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string RoomNumbers { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal RoomTotal { get; set; }
}
