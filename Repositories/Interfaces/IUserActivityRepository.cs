using BusinessObjects.DTOs;
using BusinessObjects.Entities;

namespace Repositories.Interfaces;

public interface IUserActivityRepository
{
    Task<UserLoginSession> AddLoginSessionAsync(
        UserLoginSession loginSession,
        CancellationToken cancellationToken = default);

    Task EndLoginSessionAsync(
        int loginSessionId,
        DateTime logoutAtUtc,
        CancellationToken cancellationToken = default);

    Task<UserActivityLog> AddActivityLogAsync(
        UserActivityLog activityLog,
        CancellationToken cancellationToken = default);

    Task<List<LoginSessionDto>> GetLoginSessionsByUserAsync(
        int userId,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<List<ActivityLogDto>> GetActivityLogsByUserAsync(
        int userId,
        string username,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<List<ActivityLogDto>> GetRecentSystemActivityAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<LoginLockoutStatusDto> GetLoginLockoutStatusAsync(
        string attemptedUsername,
        int? userId,
        DateTime windowStartUtc,
        CancellationToken cancellationToken = default);

    Task<ActivityDashboardDto> GetActivityDashboardAsync(
        CancellationToken cancellationToken = default);
}
