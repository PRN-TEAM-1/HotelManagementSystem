namespace BusinessObjects.DTOs;

public sealed class ServiceListItemDto
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;
}
