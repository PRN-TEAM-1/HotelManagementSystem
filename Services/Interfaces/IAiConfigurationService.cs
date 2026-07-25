using BusinessObjects.DTOs;
using BusinessObjects.Enums;

namespace Services.Interfaces;

public interface IAiConfigurationService
{
    Task<ServiceResult<List<AiProviderSettingDto>>> GetSettingsAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AiProviderSettingDto>> SaveSettingAsync(
        SaveAiProviderSettingRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<AiProviderTestResultDto>> TestSettingAsync(
        AiProviderName providerName,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);
}
