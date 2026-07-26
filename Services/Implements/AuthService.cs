using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
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

            return await BuildLoginResultAsync(
                user,
                request.ClientEnvironment,
                "LoginSuccess",
                $"Account '{user.Username}' signed in.",
                cancellationToken);
        }
        catch
        {
            return ServiceResult<LoginResultDto>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    public async Task<ServiceResult<LoginResultDto>> RestoreRememberedSessionAsync(
        RememberedLoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.UserId <= 0
            || string.IsNullOrWhiteSpace(request.Username)
            || request.UserUpdatedAtTicks <= 0
            || request.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return ServiceResult<LoginResultDto>.Failure(
                ErrorMessages.Unauthorized,
                "Saved login session has expired.");
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

            if (!IsRememberedSessionSnapshotValid(user, request))
            {
                return ServiceResult<LoginResultDto>.Failure(
                    ErrorMessages.Unauthorized,
                    "Saved login session is no longer valid.");
            }

            return await BuildLoginResultAsync(
                user!,
                request.ClientEnvironment,
                "RememberedLoginSuccess",
                $"Account '{user!.Username}' signed in from a remembered session.",
                cancellationToken);
        }
        catch
        {
            return ServiceResult<LoginResultDto>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    public async Task<ServiceResult<bool>> ValidateSessionAsync(
        CurrentSessionDto? currentSession,
        CancellationToken cancellationToken = default)
    {
        if (currentSession?.IsAuthenticated != true)
        {
            return ServiceResult<bool>.Success(false);
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(currentSession.UserId, cancellationToken);
            return ServiceResult<bool>.Success(IsCurrentSessionSnapshotValid(user, currentSession));
        }
        catch
        {
            return ServiceResult<bool>.Failure(ErrorMessages.SystemError);
        }
    }

    private async Task<ServiceResult<LoginResultDto>> BuildLoginResultAsync(
        User user,
        ClientEnvironmentDto clientEnvironment,
        string actionType,
        string activityDescription,
        CancellationToken cancellationToken)
    {
        var role = user.Role ?? await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);

        if (role is null)
        {
            return ServiceResult<LoginResultDto>.Failure(ErrorMessages.UnexpectedError);
        }

        var loginSessionResult = await _userActivityService.StartLoginSessionAsync(
            user.UserId,
            clientEnvironment,
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
            UserUpdatedAtTicks = user.UpdatedAt.Ticks,
            MachineName = loginSession?.MachineName ?? clientEnvironment.MachineName,
            WindowsUser = loginSession?.WindowsUser ?? clientEnvironment.WindowsUser,
            IpAddress = loginSession?.IpAddress ?? clientEnvironment.IpAddress,
            OsVersion = loginSession?.OsVersion ?? clientEnvironment.OsVersion,
            AppVersion = loginSession?.AppVersion ?? clientEnvironment.AppVersion,
            DeviceType = loginSession?.DeviceType ?? clientEnvironment.DeviceType
        };

        await _userActivityService.RecordActivityAsync(
            session,
            actionType,
            "Auth",
            user.UserId.ToString(),
            activityDescription,
            targetUserId: user.UserId,
            cancellationToken: cancellationToken);

        var result = new LoginResultDto
        {
            CurrentSession = session,
            WelcomeMessage = $"Welcome back, {user.FullName}."
        };

        return ServiceResult<LoginResultDto>.Success(result, result.WelcomeMessage);
    }

    private static bool IsRememberedSessionSnapshotValid(
        User? user,
        RememberedLoginRequestDto request)
    {
        return user is not null
            && user.Status == UserStatus.Active
            && user.UserId == request.UserId
            && user.Username == request.Username.Trim()
            && user.UpdatedAt.Ticks == request.UserUpdatedAtTicks;
    }

    private static bool IsCurrentSessionSnapshotValid(
        User? user,
        CurrentSessionDto currentSession)
    {
        return user is not null
            && user.Status == UserStatus.Active
            && user.UserId == currentSession.UserId
            && user.RoleId == currentSession.RoleId
            && user.Username == currentSession.Username
            && user.UpdatedAt.Ticks == currentSession.UserUpdatedAtTicks;
    }
}
