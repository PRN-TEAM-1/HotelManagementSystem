using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public sealed class UpdateUserRequestDto
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;
}
