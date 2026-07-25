using BusinessObjects.Entities;
using BusinessObjects.Enums;

namespace Repositories.Interfaces;

public interface IAiProviderSettingRepository
{
    Task<List<AiProviderSetting>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AiProviderSetting?> GetByProviderAsync(
        AiProviderName providerName,
        CancellationToken cancellationToken = default);

    Task<AiProviderSetting?> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<AiProviderSetting> SaveAsync(
        AiProviderSetting setting,
        CancellationToken cancellationToken = default);

    Task UpdateTestStatusAsync(
        AiProviderName providerName,
        DateTime testedAt,
        string status,
        CancellationToken cancellationToken = default);
}
