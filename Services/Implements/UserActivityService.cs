using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class UserActivityService : IUserActivityService
{
    private readonly IUserActivityRepository _activityRepository;
    private readonly IUserRepository _userRepository;

    public UserActivityService(
        IUserActivityRepository? activityRepository = null,
        IUserRepository? userRepository = null)
    {
        _activityRepository = activityRepository ?? new UserActivityRepository();
        _userRepository = userRepository ?? new UserRepository();
    }

    public async Task<ServiceResult<LoginSessionDto>> StartLoginSessionAsync(
        int userId,
        ClientEnvironmentDto? environment,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return ServiceResult<LoginSessionDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var env = NormalizeEnvironment(environment);
            var nowUtc = DateTime.UtcNow;
            var loginSession = new UserLoginSession
            {
                UserId = userId,
                LoginAtUtc = nowUtc,
                LastSeenAtUtc = nowUtc,
                MachineName = env.MachineName,
                WindowsUser = env.WindowsUser,
                IpAddress = env.IpAddress,
                OsVersion = env.OsVersion,
                AppVersion = env.AppVersion,
                DeviceType = env.DeviceType,
                Status = "Active"
            };

            var created = await _activityRepository.AddLoginSessionAsync(loginSession, cancellationToken);

            return ServiceResult<LoginSessionDto>.Success(new LoginSessionDto
            {
                LoginSessionId = created.LoginSessionId,
                UserId = created.UserId,
                LoginAtUtc = created.LoginAtUtc,
                LastSeenAtUtc = created.LastSeenAtUtc,
                MachineName = created.MachineName,
                WindowsUser = created.WindowsUser,
                IpAddress = created.IpAddress,
                OsVersion = created.OsVersion,
                AppVersion = created.AppVersion,
                DeviceType = created.DeviceType,
                Status = created.Status
            });
        }
        catch
        {
            return ServiceResult<LoginSessionDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<bool>> EndLoginSessionAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        if (currentUser?.LoginSessionId is not > 0)
        {
            return ServiceResult<bool>.Success(true);
        }

        try
        {
            var logoutAtUtc = DateTime.UtcNow;
            await _activityRepository.EndLoginSessionAsync(
                currentUser.LoginSessionId.Value,
                logoutAtUtc,
                cancellationToken);

            await RecordActivityAsync(
                currentUser,
                "Logout",
                "Auth",
                currentUser.UserId.ToString(),
                $"Account '{currentUser.Username}' signed out.",
                targetUserId: currentUser.UserId,
                cancellationToken: cancellationToken);

            return ServiceResult<bool>.Success(true);
        }
        catch
        {
            return ServiceResult<bool>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<bool>> RecordLoginFailureAsync(
        string attemptedUsername,
        int? userId,
        ClientEnvironmentDto? environment,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return await RecordActivityAsync(
            new ActivityLogRequestDto
            {
                ActorUserId = userId,
                TargetUserId = userId,
                AttemptedUsername = NormalizeOptional(attemptedUsername),
                ActionType = "LoginFailed",
                EntityName = "Auth",
                EntityId = userId?.ToString(),
                Description = $"Failed login attempt for username '{attemptedUsername}'.",
                Result = "Failed",
                ErrorMessage = reason,
                Environment = environment
            },
            cancellationToken);
    }

    public async Task<ServiceResult<bool>> RecordActivityAsync(
        ActivityLogRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var env = NormalizeEnvironment(request.Environment);
            var activityLog = new UserActivityLog
            {
                LoginSessionId = request.LoginSessionId,
                ActorUserId = request.ActorUserId,
                TargetUserId = request.TargetUserId,
                AttemptedUsername = NormalizeOptional(request.AttemptedUsername),
                ActionType = NormalizeRequired(request.ActionType, "Unknown"),
                EntityName = NormalizeRequired(request.EntityName, "Application"),
                EntityId = NormalizeOptional(request.EntityId),
                Description = NormalizeRequired(request.Description, "Activity recorded."),
                OldValuesJson = SanitizeJson(request.OldValuesJson),
                NewValuesJson = SanitizeJson(request.NewValuesJson),
                Result = string.Equals(request.Result, "Failed", StringComparison.OrdinalIgnoreCase)
                    ? "Failed"
                    : "Success",
                ErrorMessage = NormalizeOptional(request.ErrorMessage),
                OccurredAtUtc = DateTime.UtcNow,
                MachineName = env.MachineName,
                IpAddress = env.IpAddress,
                DeviceType = env.DeviceType
            };

            await _activityRepository.AddActivityLogAsync(activityLog, cancellationToken);
            return ServiceResult<bool>.Success(true);
        }
        catch
        {
            return ServiceResult<bool>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<bool>> RecordActivityAsync(
        CurrentSessionDto? currentUser,
        string actionType,
        string entityName,
        string? entityId,
        string description,
        int? targetUserId = null,
        string result = "Success",
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        if (currentUser is null)
        {
            return ServiceResult<bool>.Success(true);
        }

        return await RecordActivityAsync(
            new ActivityLogRequestDto
            {
                LoginSessionId = currentUser.LoginSessionId,
                ActorUserId = currentUser.UserId,
                TargetUserId = targetUserId,
                ActionType = actionType,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                Result = result,
                ErrorMessage = errorMessage,
                Environment = ToEnvironment(currentUser)
            },
            cancellationToken);
    }

    public async Task<ServiceResult<List<LoginSessionDto>>> GetUserLoginSessionsAsync(
        int userId,
        CurrentSessionDto? currentUser,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanViewUserAudit<List<LoginSessionDto>>(userId, currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var sessions = await _activityRepository.GetLoginSessionsByUserAsync(userId, take, cancellationToken);
            return ServiceResult<List<LoginSessionDto>>.Success(sessions);
        }
        catch
        {
            return ServiceResult<List<LoginSessionDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<ActivityLogDto>>> GetUserActivityLogsAsync(
        int userId,
        CurrentSessionDto? currentUser,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanViewUserAudit<List<ActivityLogDto>>(userId, currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                return ServiceResult<List<ActivityLogDto>>.Failure(ErrorMessages.NotFound);
            }

            var logs = await _activityRepository.GetActivityLogsByUserAsync(
                userId,
                user.Username,
                take,
                cancellationToken);

            return ServiceResult<List<ActivityLogDto>>.Success(logs);
        }
        catch
        {
            return ServiceResult<List<ActivityLogDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<ActivityLogDto>>> GetRecentSystemActivityAsync(
        CurrentSessionDto? currentUser,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureAdmin<List<ActivityLogDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var logs = await _activityRepository.GetRecentSystemActivityAsync(take, cancellationToken);
            return ServiceResult<List<ActivityLogDto>>.Success(logs);
        }
        catch
        {
            return ServiceResult<List<ActivityLogDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<LoginLockoutStatusDto>> GetLoginLockoutStatusAsync(
        string attemptedUsername,
        int? userId,
        DateTime windowStartUtc,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(attemptedUsername))
        {
            return ServiceResult<LoginLockoutStatusDto>.Success(new LoginLockoutStatusDto());
        }

        try
        {
            var status = await _activityRepository.GetLoginLockoutStatusAsync(
                attemptedUsername.Trim(),
                userId,
                windowStartUtc,
                cancellationToken);

            return ServiceResult<LoginLockoutStatusDto>.Success(status);
        }
        catch
        {
            return ServiceResult<LoginLockoutStatusDto>.Failure(ErrorMessages.DatabaseConnectionRequired);
        }
    }

    public async Task<ServiceResult<ActivityDashboardDto>> GetActivityDashboardAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureAdmin<ActivityDashboardDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var dashboard = await _activityRepository.GetActivityDashboardAsync(cancellationToken);
            return ServiceResult<ActivityDashboardDto>.Success(dashboard);
        }
        catch
        {
            return ServiceResult<ActivityDashboardDto>.Failure(ErrorMessages.SystemError);
        }
    }

    private static ServiceResult<T>? EnsureCanViewUserAudit<T>(int userId, CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        if (currentUser.RoleName == RoleName.Admin || currentUser.UserId == userId)
        {
            return null;
        }

        return ServiceResult<T>.Failure(ErrorMessages.Forbidden);
    }

    private static ServiceResult<T>? EnsureAdmin<T>(CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        return currentUser.RoleName == RoleName.Admin
            ? null
            : ServiceResult<T>.Failure(ErrorMessages.Forbidden);
    }

    private static ClientEnvironmentDto ToEnvironment(CurrentSessionDto currentUser)
    {
        return new ClientEnvironmentDto
        {
            MachineName = currentUser.MachineName,
            WindowsUser = currentUser.WindowsUser,
            IpAddress = currentUser.IpAddress,
            OsVersion = currentUser.OsVersion,
            AppVersion = currentUser.AppVersion,
            DeviceType = currentUser.DeviceType
        };
    }

    private static ClientEnvironmentDto NormalizeEnvironment(ClientEnvironmentDto? environment)
    {
        return new ClientEnvironmentDto
        {
            MachineName = NormalizeRequired(environment?.MachineName, "Unknown"),
            WindowsUser = NormalizeRequired(environment?.WindowsUser, "Unknown"),
            IpAddress = NormalizeRequired(environment?.IpAddress, "Unknown"),
            OsVersion = NormalizeRequired(environment?.OsVersion, "Unknown"),
            AppVersion = NormalizeRequired(environment?.AppVersion, "Unknown"),
            DeviceType = NormalizeRequired(environment?.DeviceType, "Windows Desktop")
        };
    }

    private static string NormalizeRequired(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? SanitizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var normalized = json.Trim();

        return normalized.Contains("password", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("api", StringComparison.OrdinalIgnoreCase)
               || normalized.Contains("key", StringComparison.OrdinalIgnoreCase)
            ? "{ \"redacted\": true }"
            : normalized;
    }
}
