using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class UserActivityRepository : IUserActivityRepository
{
    private readonly UserActivityDao _dao;

    public UserActivityRepository(UserActivityDao? dao = null)
    {
        _dao = dao ?? new UserActivityDao();
    }

    public Task<UserLoginSession> AddLoginSessionAsync(
        UserLoginSession loginSession,
        CancellationToken cancellationToken = default)
    {
        return _dao.AddLoginSessionAsync(loginSession, cancellationToken);
    }

    public Task EndLoginSessionAsync(
        int loginSessionId,
        DateTime logoutAtUtc,
        CancellationToken cancellationToken = default)
    {
        return _dao.EndLoginSessionAsync(loginSessionId, logoutAtUtc, cancellationToken);
    }

    public Task<UserActivityLog> AddActivityLogAsync(
        UserActivityLog activityLog,
        CancellationToken cancellationToken = default)
    {
        return _dao.AddActivityLogAsync(activityLog, cancellationToken);
    }

    public Task<List<LoginSessionDto>> GetLoginSessionsByUserAsync(
        int userId,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return _dao.GetLoginSessionsByUserAsync(userId, take, cancellationToken);
    }

    public Task<List<ActivityLogDto>> GetActivityLogsByUserAsync(
        int userId,
        string username,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return _dao.GetActivityLogsByUserAsync(userId, username, take, cancellationToken);
    }

    public Task<List<ActivityLogDto>> GetRecentSystemActivityAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return _dao.GetRecentSystemActivityAsync(take, cancellationToken);
    }

    public Task<LoginLockoutStatusDto> GetLoginLockoutStatusAsync(
        string attemptedUsername,
        int? userId,
        DateTime windowStartUtc,
        CancellationToken cancellationToken = default)
    {
        return _dao.GetLoginLockoutStatusAsync(
            attemptedUsername,
            userId,
            windowStartUtc,
            cancellationToken);
    }

    public Task<ActivityDashboardDto> GetActivityDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        return _dao.GetActivityDashboardAsync(cancellationToken);
    }
}
