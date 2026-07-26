using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class UserActivityDao
{
    private const int DefaultTake = 100;

    public async Task<UserLoginSession> AddLoginSessionAsync(
        UserLoginSession loginSession,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        context.UserLoginSessions.Add(loginSession);
        await context.SaveChangesAsync(cancellationToken);

        return loginSession;
    }

    public async Task EndLoginSessionAsync(
        int loginSessionId,
        DateTime logoutAtUtc,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var session = await context.UserLoginSessions
            .FirstOrDefaultAsync(row => row.LoginSessionId == loginSessionId, cancellationToken);

        if (session is null)
        {
            return;
        }

        session.LogoutAtUtc = logoutAtUtc;
        session.LastSeenAtUtc = logoutAtUtc;
        session.Status = "LoggedOut";

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserActivityLog> AddActivityLogAsync(
        UserActivityLog activityLog,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        context.UserActivityLogs.Add(activityLog);
        await context.SaveChangesAsync(cancellationToken);

        return activityLog;
    }

    public async Task<List<LoginSessionDto>> GetLoginSessionsByUserAsync(
        int userId,
        int take = DefaultTake,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.UserLoginSessions
            .AsNoTracking()
            .Include(session => session.User)
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.LoginAtUtc)
            .Take(NormalizeTake(take))
            .Select(session => new LoginSessionDto
            {
                LoginSessionId = session.LoginSessionId,
                UserId = session.UserId,
                Username = session.User == null ? string.Empty : session.User.Username,
                FullName = session.User == null ? string.Empty : session.User.FullName,
                LoginAtUtc = session.LoginAtUtc,
                LogoutAtUtc = session.LogoutAtUtc,
                LastSeenAtUtc = session.LastSeenAtUtc,
                MachineName = session.MachineName,
                WindowsUser = session.WindowsUser,
                IpAddress = session.IpAddress,
                OsVersion = session.OsVersion,
                AppVersion = session.AppVersion,
                DeviceType = session.DeviceType,
                Status = session.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ActivityLogDto>> GetActivityLogsByUserAsync(
        int userId,
        string username,
        int take = DefaultTake,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var normalizedUsername = username.Trim();

        var logs = context.UserActivityLogs
            .AsNoTracking()
            .Include(log => log.ActorUser)
            .Include(log => log.TargetUser)
            .Include(log => log.LoginSession)
            .Where(log =>
                log.ActorUserId == userId
                || log.TargetUserId == userId
                || (log.LoginSession != null && log.LoginSession.UserId == userId)
                || (!string.IsNullOrWhiteSpace(normalizedUsername)
                    && log.AttemptedUsername == normalizedUsername));

        return await MapLogQuery(logs)
            .OrderByDescending(log => log.OccurredAtUtc)
            .Take(NormalizeTake(take))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ActivityLogDto>> GetRecentSystemActivityAsync(
        int take = DefaultTake,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var logs = context.UserActivityLogs
            .AsNoTracking()
            .Include(log => log.ActorUser)
            .Include(log => log.TargetUser);

        return await MapLogQuery(logs)
            .OrderByDescending(log => log.OccurredAtUtc)
            .Take(NormalizeTake(take))
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityDashboardDto> GetActivityDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var todayLocal = DateTime.Now.Date;
        var tomorrowLocal = todayLocal.AddDays(1);
        var trendStartLocal = todayLocal.AddDays(-6);
        var todayUtc = todayLocal.ToUniversalTime();
        var tomorrowUtc = tomorrowLocal.ToUniversalTime();
        var trendStartUtc = trendStartLocal.ToUniversalTime();

        var dashboard = new ActivityDashboardDto
        {
            TodayLoginCount = await context.UserLoginSessions
                .AsNoTracking()
                .CountAsync(session => session.LoginAtUtc >= todayUtc && session.LoginAtUtc < tomorrowUtc, cancellationToken),
            TodayFailedLoginCount = await context.UserActivityLogs
                .AsNoTracking()
                .CountAsync(log => log.ActionType == "LoginFailed" && log.OccurredAtUtc >= todayUtc && log.OccurredAtUtc < tomorrowUtc, cancellationToken),
            TodayLogoutCount = await context.UserActivityLogs
                .AsNoTracking()
                .CountAsync(log => log.ActionType == "Logout" && log.OccurredAtUtc >= todayUtc && log.OccurredAtUtc < tomorrowUtc, cancellationToken),
            TodayActivityCount = await context.UserActivityLogs
                .AsNoTracking()
                .CountAsync(log => log.OccurredAtUtc >= todayUtc && log.OccurredAtUtc < tomorrowUtc, cancellationToken)
        };

        var dailyCounts = new Dictionary<DateTime, int>();
        for (var date = trendStartLocal; date <= todayLocal; date = date.AddDays(1))
        {
            var dateUtc = date.ToUniversalTime();
            var nextDateUtc = date.AddDays(1).ToUniversalTime();
            dailyCounts[date] = await context.UserActivityLogs
                .AsNoTracking()
                .CountAsync(log => log.OccurredAtUtc >= dateUtc && log.OccurredAtUtc < nextDateUtc, cancellationToken);
        }

        dashboard.DailyActivity = dailyCounts
            .Select(row => new ActivityTrendDto
            {
                Date = row.Key,
                ActivityCount = row.Value
            })
            .ToList();
        dashboard.MaxDailyActivityCount = Math.Max(1, dashboard.DailyActivity.Max(row => row.ActivityCount));

        dashboard.TopActiveUsers = await context.UserActivityLogs
            .AsNoTracking()
            .Where(log => log.ActorUserId != null && log.OccurredAtUtc >= trendStartUtc)
            .GroupBy(log => log.ActorUserId!.Value)
            .Select(group => new
            {
                UserId = group.Key,
                ActivityCount = group.Count()
            })
            .OrderByDescending(row => row.ActivityCount)
            .Take(5)
            .Join(
                context.Users.AsNoTracking(),
                row => row.UserId,
                user => user.UserId,
                (row, user) => new TopActiveUserDto
                {
                    UserId = row.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    ActivityCount = row.ActivityCount
                })
            .ToListAsync(cancellationToken);

        dashboard.RecentActivities = await GetRecentSystemActivityAsync(DefaultTake, cancellationToken);

        return dashboard;
    }

    private static IQueryable<ActivityLogDto> MapLogQuery(IQueryable<UserActivityLog> logs)
    {
        return logs.Select(log => new ActivityLogDto
        {
            ActivityLogId = log.ActivityLogId,
            LoginSessionId = log.LoginSessionId,
            ActorUserId = log.ActorUserId,
            ActorUsername = log.ActorUser == null ? string.Empty : log.ActorUser.Username,
            ActorFullName = log.ActorUser == null ? string.Empty : log.ActorUser.FullName,
            TargetUserId = log.TargetUserId,
            TargetUsername = log.TargetUser == null ? string.Empty : log.TargetUser.Username,
            AttemptedUsername = log.AttemptedUsername,
            ActionType = log.ActionType,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            Description = log.Description,
            Result = log.Result,
            ErrorMessage = log.ErrorMessage,
            OccurredAtUtc = log.OccurredAtUtc,
            MachineName = log.MachineName,
            IpAddress = log.IpAddress,
            DeviceType = log.DeviceType
        });
    }

    private static int NormalizeTake(int take)
    {
        return Math.Clamp(take, 1, DefaultTake);
    }
}
