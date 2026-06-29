using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects;

public static class DbContextFactory
{
    private const string AppSettingsFileName = "appsettings.json";
    private const string ConnectionStringsSectionName = "ConnectionStrings";
    private const string DefaultConnectionName = "DefaultConnection";
    private const string LegacyDefaultConnectionName = "DefaultConnectionString";
    private const string WpfFolderName = "WPF";
    private const int SearchDepthLimit = 8;

    public static string GetConnectionString(string connectionName = DefaultConnectionName)
    {
        var appSettingsPath = GetAppSettingsPath();

        using var stream = File.OpenRead(appSettingsPath);
        using var document = JsonDocument.Parse(
            stream,
            new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

        if (!document.RootElement.TryGetProperty(ConnectionStringsSectionName, out var connectionStringsSection))
        {
            throw new InvalidOperationException(
                $"The '{ConnectionStringsSectionName}' section was not found in '{appSettingsPath}'.");
        }

        if (!TryGetConnectionStringElement(connectionStringsSection, connectionName, out var connectionStringElement))
        {
            throw new InvalidOperationException(
                $"The connection string '{connectionName}' was not found in '{appSettingsPath}'. " +
                $"Supported keys: '{DefaultConnectionName}' and '{LegacyDefaultConnectionName}'.");
        }

        var connectionString = connectionStringElement.GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"The connection string '{connectionName}' in '{appSettingsPath}' is empty.");
        }

        return connectionString.Trim();
    }

    public static HotelManagementContext CreateDbContext(string connectionName = DefaultConnectionName)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelManagementContext>();
        optionsBuilder.UseSqlServer(GetConnectionString(connectionName));
        return new HotelManagementContext(optionsBuilder.Options);
    }

    public static string GetAppSettingsPath()
    {
        foreach (var candidatePath in GetCandidatePaths())
        {
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new FileNotFoundException(
            "Unable to locate appsettings.json. Expected it in the application output folder or the WPF project folder.");
    }

    private static IEnumerable<string> GetCandidatePaths()
    {
        var candidatePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddCandidatePaths(candidatePaths, AppContext.BaseDirectory);
        AddCandidatePaths(candidatePaths, Directory.GetCurrentDirectory());

        return candidatePaths;
    }

    private static void AddCandidatePaths(ISet<string> candidatePaths, string basePath)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            return;
        }

        var currentDirectory = new DirectoryInfo(Path.GetFullPath(basePath));

        for (var depth = 0; currentDirectory is not null && depth < SearchDepthLimit; depth++)
        {
            candidatePaths.Add(Path.Combine(currentDirectory.FullName, AppSettingsFileName));
            candidatePaths.Add(Path.Combine(currentDirectory.FullName, WpfFolderName, AppSettingsFileName));

            currentDirectory = currentDirectory.Parent;
        }
    }

    private static bool TryGetConnectionStringElement(
        JsonElement connectionStringsSection,
        string connectionName,
        out JsonElement connectionStringElement)
    {
        if (connectionStringsSection.TryGetProperty(connectionName, out connectionStringElement))
        {
            return true;
        }

        if (string.Equals(connectionName, DefaultConnectionName, StringComparison.OrdinalIgnoreCase))
        {
            return connectionStringsSection.TryGetProperty(LegacyDefaultConnectionName, out connectionStringElement);
        }

        if (string.Equals(connectionName, LegacyDefaultConnectionName, StringComparison.OrdinalIgnoreCase))
        {
            return connectionStringsSection.TryGetProperty(DefaultConnectionName, out connectionStringElement);
        }

        return false;
    }
}
