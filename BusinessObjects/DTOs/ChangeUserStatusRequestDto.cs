using BusinessObjects.Enums;

namespace BusinessObjects.DTOs;

public sealed class ChangeUserStatusRequestDto
{
    public int UserId { get; set; }

    public UserStatus Status { get; set; }
}
