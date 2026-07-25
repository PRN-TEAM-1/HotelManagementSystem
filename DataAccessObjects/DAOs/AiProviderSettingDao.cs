using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class AiProviderSettingDao
{
    public async Task<List<AiProviderSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();
        await EnsureSchemaAsync(context, cancellationToken);

        return await context.AiProviderSettings
            .AsNoTracking()
            .OrderBy(setting => setting.ProviderName)
            .ToListAsync(cancellationToken);
    }

    public async Task<AiProviderSetting?> GetByProviderAsync(
        AiProviderName providerName,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();
        await EnsureSchemaAsync(context, cancellationToken);

        return await context.AiProviderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(setting => setting.ProviderName == providerName, cancellationToken);
    }

    public async Task<AiProviderSetting?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();
        await EnsureSchemaAsync(context, cancellationToken);

        return await context.AiProviderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(setting => setting.IsActive, cancellationToken);
    }

    public async Task<AiProviderSetting> SaveAsync(
        AiProviderSetting setting,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setting);

        await using var context = DbContextFactory.CreateDbContext();
        await EnsureSchemaAsync(context, cancellationToken);

        var now = DateTime.Now;
        var existing = await context.AiProviderSettings
            .FirstOrDefaultAsync(item => item.ProviderName == setting.ProviderName, cancellationToken);

        if (setting.IsActive)
        {
            var activeSettings = await context.AiProviderSettings
                .Where(item => item.ProviderName != setting.ProviderName && item.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var activeSetting in activeSettings)
            {
                activeSetting.IsActive = false;
                activeSetting.UpdatedAt = now;
            }
        }

        if (existing is null)
        {
            setting.CreatedAt = now;
            setting.UpdatedAt = now;
            context.AiProviderSettings.Add(setting);
        }
        else
        {
            existing.ModelName = setting.ModelName;
            existing.EncryptedApiKey = setting.EncryptedApiKey;
            existing.EndpointUrl = setting.EndpointUrl;
            existing.Temperature = setting.Temperature;
            existing.MaxOutputTokens = setting.MaxOutputTokens;
            existing.TimeoutSeconds = setting.TimeoutSeconds;
            existing.IsActive = setting.IsActive;
            existing.UpdatedAt = now;
            setting = existing;
        }

        await context.SaveChangesAsync(cancellationToken);

        return setting;
    }

    public async Task UpdateTestStatusAsync(
        AiProviderName providerName,
        DateTime testedAt,
        string status,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();
        await EnsureSchemaAsync(context, cancellationToken);

        var existing = await context.AiProviderSettings
            .FirstOrDefaultAsync(item => item.ProviderName == providerName, cancellationToken);

        if (existing is null)
        {
            return;
        }

        existing.LastTestedAt = testedAt;
        existing.LastTestStatus = status.Length > 500 ? status[..500] : status;
        existing.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureSchemaAsync(
        HotelManagementContext context,
        CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.ai_provider_settings', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ai_provider_settings
                (
                    ai_provider_setting_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    provider_name NVARCHAR(30) NOT NULL,
                    model_name NVARCHAR(100) NOT NULL,
                    encrypted_api_key NVARCHAR(4000) NOT NULL CONSTRAINT DF_ai_provider_settings_api_key DEFAULT (N''),
                    endpoint_url NVARCHAR(500) NULL,
                    temperature DECIMAL(5,2) NOT NULL CONSTRAINT DF_ai_provider_settings_temperature DEFAULT (0.20),
                    max_output_tokens INT NOT NULL CONSTRAINT DF_ai_provider_settings_max_output_tokens DEFAULT (900),
                    timeout_seconds INT NOT NULL CONSTRAINT DF_ai_provider_settings_timeout_seconds DEFAULT (45),
                    is_active BIT NOT NULL CONSTRAINT DF_ai_provider_settings_is_active DEFAULT (0),
                    last_tested_at DATETIME2 NULL,
                    last_test_status NVARCHAR(500) NULL,
                    created_at DATETIME2 NOT NULL CONSTRAINT DF_ai_provider_settings_created_at DEFAULT SYSUTCDATETIME(),
                    updated_at DATETIME2 NOT NULL CONSTRAINT DF_ai_provider_settings_updated_at DEFAULT SYSUTCDATETIME()
                );
            END

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ai_provider_settings_provider_name' AND object_id = OBJECT_ID(N'dbo.ai_provider_settings'))
            BEGIN
                CREATE UNIQUE INDEX UX_ai_provider_settings_provider_name ON dbo.ai_provider_settings(provider_name);
            END

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ai_provider_settings_active' AND object_id = OBJECT_ID(N'dbo.ai_provider_settings'))
            BEGIN
                CREATE UNIQUE INDEX UX_ai_provider_settings_active ON dbo.ai_provider_settings(is_active) WHERE is_active = 1;
            END
            """, cancellationToken);
    }
}
