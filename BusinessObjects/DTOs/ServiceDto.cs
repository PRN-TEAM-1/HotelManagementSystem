namespace BusinessObjects.DTOs;

public sealed class ServiceDto
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
