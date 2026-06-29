using System.Security.Cryptography;
using System.Text;

namespace DataAccessObjects.Security;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        if (password is null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash);
    }

    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        return string.Equals(HashPassword(password), storedHash.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
