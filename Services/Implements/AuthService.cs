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
    private readonly IUserActivityService _userActivityService;

    public AuthService(
        IUserRepository? userRepository = null,
        IRoleRepository? roleRepository = null,
        IUserActivityService? userActivityService = null)
    {
        _userRepository = userRepository ?? new UserRepository();
        _roleRepository = roleRepository ?? new RoleRepository();
        _userActivityService = userActivityService ?? new UserActivityService();
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
                await _userActivityService.RecordLoginFailureAsync(
                    username,
                    user?.UserId,
                    request.ClientEnvironment,
                    ErrorMessages.InvalidCredentials,
                    cancellationToken);

                return ServiceResult<LoginResultDto>.Failure(ErrorMessages.InvalidCredentials);
            }

            if (user.Status == UserStatus.Inactive)
            {
                await _userActivityService.RecordLoginFailureAsync(
                    username,
                    user.UserId,
                    request.ClientEnvironment,
                    ErrorMessages.AccountInactive,
                    cancellationToken);

                return ServiceResult<LoginResultDto>.Failure(ErrorMessages.AccountInactive);
            }

            var role = user.Role ?? await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);

            if (role is null)
            {
                return ServiceResult<LoginResultDto>.Failure(ErrorMessages.UnexpectedError);
            }

            var loginSessionResult = await _userActivityService.StartLoginSessionAsync(
                user.UserId,
                request.ClientEnvironment,
                cancellationToken);
            var loginSession = loginSessionResult.Data;

            var session = new CurrentSessionDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                LoginSessionId = loginSession?.LoginSessionId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = role.Name,
                LoggedInAtUtc = loginSession?.LoginAtUtc ?? DateTime.UtcNow,
                MachineName = loginSession?.MachineName ?? request.ClientEnvironment.MachineName,
                WindowsUser = loginSession?.WindowsUser ?? request.ClientEnvironment.WindowsUser,
                IpAddress = loginSession?.IpAddress ?? request.ClientEnvironment.IpAddress,
                OsVersion = loginSession?.OsVersion ?? request.ClientEnvironment.OsVersion,
                AppVersion = loginSession?.AppVersion ?? request.ClientEnvironment.AppVersion,
                DeviceType = loginSession?.DeviceType ?? request.ClientEnvironment.DeviceType
            };

            await _userActivityService.RecordActivityAsync(
                session,
                "LoginSuccess",
                "Auth",
                user.UserId.ToString(),
                $"Account '{user.Username}' signed in.",
                targetUserId: user.UserId,
                cancellationToken: cancellationToken);

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
