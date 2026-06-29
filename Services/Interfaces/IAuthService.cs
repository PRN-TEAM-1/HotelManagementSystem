using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResultDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}
