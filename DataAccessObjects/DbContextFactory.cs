using System.Text.Json;

namespace DataAccessObjects;

public static class DbContextFactory
{
    private const string AppSettingsFileName = "appsettings.json";
    private const string ConnectionStringsSectionName = "ConnectionStrings";
    private const string DefaultConnectionName = "DefaultConnection";
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

        if (!connectionStringsSection.TryGetProperty(connectionName, out var connectionStringElement))
        {
            throw new InvalidOperationException(
                $"The connection string '{connectionName}' was not found in '{appSettingsPath}'.");
        }

        var connectionString = connectionStringElement.GetString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"The connection string '{connectionName}' in '{appSettingsPath}' is empty.");
        }

        return connectionString.Trim();
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
}
