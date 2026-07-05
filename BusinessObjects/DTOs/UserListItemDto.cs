namespace BusinessObjects.DTOs;

public sealed class UserListItemDto
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool HasBusinessReferences { get; set; }
}
