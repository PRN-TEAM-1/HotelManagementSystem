using BusinessObjects.Entities;
using BusinessObjects.Enums;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class AiProviderSettingRepository : IAiProviderSettingRepository
{
    private readonly AiProviderSettingDao _dao;

    public AiProviderSettingRepository(AiProviderSettingDao? dao = null)
    {
        _dao = dao ?? new AiProviderSettingDao();
    }

    public async Task<List<AiProviderSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dao.GetAllAsync(cancellationToken);
    }

    public async Task<AiProviderSetting?> GetByProviderAsync(
        AiProviderName providerName,
        CancellationToken cancellationToken = default)
    {
        return await _dao.GetByProviderAsync(providerName, cancellationToken);
    }

    public async Task<AiProviderSetting?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dao.GetActiveAsync(cancellationToken);
    }

    public async Task<AiProviderSetting> SaveAsync(
        AiProviderSetting setting,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(setting);

        return await _dao.SaveAsync(setting, cancellationToken);
    }

    public async Task UpdateTestStatusAsync(
        AiProviderName providerName,
        DateTime testedAt,
        string status,
        CancellationToken cancellationToken = default)
    {
        await _dao.UpdateTestStatusAsync(providerName, testedAt, status, cancellationToken);
    }
}
