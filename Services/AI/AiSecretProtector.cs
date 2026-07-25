using System.Security.Cryptography;
using System.Text;

namespace Services.AI;

internal static class AiSecretProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const string KeyPurpose = "HotelManagementSystem.AiProviderSettings.v1";

    public static string Protect(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var plaintext = Encoding.UTF8.GetBytes(value.Trim());
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(GetMachineScopedKey(), TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var payload = new byte[NonceSize + TagSize + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, NonceSize, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, payload, NonceSize + TagSize, ciphertext.Length);

        return Convert.ToBase64String(payload);
    }

    public static string Unprotect(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
        {
            return string.Empty;
        }

        try
        {
            var payload = Convert.FromBase64String(protectedValue);
            if (payload.Length <= NonceSize + TagSize)
            {
                return string.Empty;
            }

            var nonce = payload[..NonceSize];
            var tag = payload[NonceSize..(NonceSize + TagSize)];
            var ciphertext = payload[(NonceSize + TagSize)..];
            var plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(GetMachineScopedKey(), TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch
        {
            return protectedValue.Trim();
        }
    }

    private static byte[] GetMachineScopedKey()
    {
        var seed = string.Join(
            "|",
            KeyPurpose,
            Environment.UserDomainName,
            Environment.UserName,
            Environment.MachineName);

        return SHA256.HashData(Encoding.UTF8.GetBytes(seed));
    }
}
