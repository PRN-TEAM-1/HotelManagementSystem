using BusinessObjects.Enums;

namespace BusinessObjects.Entities;

public sealed class Role
{
    public int RoleId { get; set; }

    public RoleName Name { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
