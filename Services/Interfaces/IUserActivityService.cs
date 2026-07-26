using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IUserActivityService
{
    Task<ServiceResult<LoginSessionDto>> StartLoginSessionAsync(
        int userId,
        ClientEnvironmentDto? environment,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> EndLoginSessionAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> RecordLoginFailureAsync(
        string attemptedUsername,
        int? userId,
        ClientEnvironmentDto? environment,
        string reason,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> RecordActivityAsync(
        ActivityLogRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> RecordActivityAsync(
        CurrentSessionDto? currentUser,
        string actionType,
        string entityName,
        string? entityId,
        string description,
        int? targetUserId = null,
        string result = "Success",
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<LoginSessionDto>>> GetUserLoginSessionsAsync(
        int userId,
        CurrentSessionDto? currentUser,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<ActivityLogDto>>> GetUserActivityLogsAsync(
        int userId,
        CurrentSessionDto? currentUser,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<ActivityLogDto>>> GetRecentSystemActivityAsync(
        CurrentSessionDto? currentUser,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<ActivityDashboardDto>> GetActivityDashboardAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);
}
