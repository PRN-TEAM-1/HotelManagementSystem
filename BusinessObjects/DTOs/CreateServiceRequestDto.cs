namespace BusinessObjects.DTOs;

public sealed class CreateServiceRequestDto
{
    public string ServiceName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
