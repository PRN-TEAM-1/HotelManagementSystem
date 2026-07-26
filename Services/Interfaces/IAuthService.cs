using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResultDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<LoginResultDto>> RestoreRememberedSessionAsync(
        RememberedLoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ValidateSessionAsync(
        CurrentSessionDto? currentSession,
        CancellationToken cancellationToken = default);
}
