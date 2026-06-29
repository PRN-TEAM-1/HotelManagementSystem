using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using DataAccessObjects.Security;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AuthService(
        IUserRepository? userRepository = null,
        IRoleRepository? roleRepository = null)
    {
        _userRepository = userRepository ?? new UserRepository();
        _roleRepository = roleRepository ?? new RoleRepository();
    }

    public async Task<ServiceResult<LoginResultDto>> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var username = request.Username?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<LoginResultDto>.Failure(
                ErrorMessages.ValidationFailed,
                "Username and password are required.");
        }

        try
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

            if (user is null || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                return ServiceResult<LoginResultDto>.Failure(ErrorMessages.InvalidCredentials);
            }

            if (user.Status == UserStatus.Inactive)
            {
                return ServiceResult<LoginResultDto>.Failure(ErrorMessages.AccountInactive);
            }

            if (user.Status == UserStatus.Locked)
            {
                return ServiceResult<LoginResultDto>.Failure(ErrorMessages.AccountLocked);
            }

            var role = user.Role ?? await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);

            if (role is null)
            {
                return ServiceResult<LoginResultDto>.Failure(ErrorMessages.UnexpectedError);
            }

            var session = new CurrentSessionDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = role.Name,
                LoggedInAtUtc = DateTime.UtcNow
            };

            var result = new LoginResultDto
            {
                CurrentSession = session,
                WelcomeMessage = $"Welcome back, {user.FullName}."
            };

            return ServiceResult<LoginResultDto>.Success(result, result.WelcomeMessage);
        }
        catch
        {
            return ServiceResult<LoginResultDto>.Failure(ErrorMessages.UnexpectedError);
        }
    }
}
