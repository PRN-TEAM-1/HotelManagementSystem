namespace BusinessObjects.DTOs;

public sealed class AiServiceRecommendationContextDto
{
    public int BookingDetailId { get; set; }

    public int BookingId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string RoomNumber { get; set; } = string.Empty;

    public string RoomType { get; set; } = string.Empty;

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public DateTime? ActualCheckInDate { get; set; }

    public int NumberOfNights { get; set; }

    public decimal RoomPrice { get; set; }

    public decimal RoomTotal { get; set; }

    public string BookingNote { get; set; } = string.Empty;

    public string StayNote { get; set; } = string.Empty;

    public string CheckInNote { get; set; } = string.Empty;

    public int GuestStayCount { get; set; }

    public decimal CurrentServiceTotal { get; set; }

    public List<AiCatalogServiceDto> ActiveServices { get; set; } = new();

    public List<AiExistingServiceOrderDto> ExistingServiceOrders { get; set; } = new();

    public List<AiGuestServiceHistoryDto> GuestServiceHistory { get; set; } = new();
}

public sealed class AiCatalogServiceDto
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }
}

public sealed class AiExistingServiceOrderDto
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime OrderDate { get; set; }
}

public sealed class AiGuestServiceHistoryDto
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int TimesOrdered { get; set; }

    public int TotalQuantity { get; set; }

    public DateTime LastOrderedAt { get; set; }
}
