namespace BusinessObjects.Entities;

public sealed class Customer
{
    public int CustomerId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? IdentityCard { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
