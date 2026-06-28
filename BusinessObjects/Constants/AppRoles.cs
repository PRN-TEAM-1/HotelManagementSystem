using BusinessObjects.Enums;

namespace BusinessObjects.Constants;

public static class AppRoles
{
    public const string Admin = nameof(RoleName.Admin);
    public const string Manager = nameof(RoleName.Manager);
    public const string Receptionist = nameof(RoleName.Receptionist);

    public static readonly string[] All = [Admin, Manager, Receptionist];

    public static bool IsValid(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        return All.Any(role => string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase));
    }
}
