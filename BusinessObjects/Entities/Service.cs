using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ServiceStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
