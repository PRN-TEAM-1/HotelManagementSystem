using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BusinessObjects.DTOs;

namespace WPF.Helpers;

public sealed class RememberedLoginStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("HotelManagementSystem.RememberedLogin.v1");

    private readonly string _filePath;

    public RememberedLoginStore(string? filePath = null)
    {
        _filePath = string.IsNullOrWhiteSpace(filePath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HotelManagementSystem",
                "remembered-login.dat")
            : filePath;
    }

    public void Save(CurrentSessionDto session, TimeSpan lifetime)
    {
        if (session.UserId <= 0 || session.UserUpdatedAtTicks <= 0)
        {
            Clear();
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var record = new RememberedLoginRecord
            {
                UserId = session.UserId,
                Username = session.Username,
                UserUpdatedAtTicks = session.UserUpdatedAtTicks,
                ExpiresAtUtc = DateTime.UtcNow.Add(lifetime)
            };

            var json = JsonSerializer.Serialize(record);
            var protectedBytes = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(json),
                Entropy,
                DataProtectionScope.CurrentUser);

            File.WriteAllBytes(_filePath, protectedBytes);
        }
        catch
        {
            Clear();
        }
    }

    public bool TryLoad(out RememberedLoginRecord? record)
    {
        record = null;

        try
        {
            if (!File.Exists(_filePath))
            {
                return false;
            }

            var protectedBytes = File.ReadAllBytes(_filePath);
            var jsonBytes = ProtectedData.Unprotect(
                protectedBytes,
                Entropy,
                DataProtectionScope.CurrentUser);

            record = JsonSerializer.Deserialize<RememberedLoginRecord>(
                Encoding.UTF8.GetString(jsonBytes));

            if (record is null
                || record.Version != RememberedLoginRecord.CurrentVersion
                || record.UserId <= 0
                || string.IsNullOrWhiteSpace(record.Username)
                || record.UserUpdatedAtTicks <= 0
                || record.ExpiresAtUtc <= DateTime.UtcNow)
            {
                Clear();
                record = null;
                return false;
            }

            return true;
        }
        catch
        {
            Clear();
            record = null;
            return false;
        }
    }

    public void Clear()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
        catch
        {
            // Ignore cleanup errors; the next load attempt will validate the token again.
        }
    }
}

public sealed class RememberedLoginRecord
{
    public const int CurrentVersion = 1;

    public int Version { get; init; } = CurrentVersion;

    public int UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public long UserUpdatedAtTicks { get; init; }

    public DateTime ExpiresAtUtc { get; init; }
}
