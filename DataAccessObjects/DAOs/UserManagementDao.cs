using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class UserManagementDao
{
    public async Task<List<User>> GetUsersAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var query = context.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";
            query = query.Where(user =>
                EF.Functions.Like(user.Username, pattern)
                || EF.Functions.Like(user.FullName, pattern)
                || EF.Functions.Like(user.Email, pattern)
                || (user.PhoneNumber != null && EF.Functions.Like(user.PhoneNumber, pattern)));
        }

        return await query
            .OrderBy(user => user.RoleId)
            .ThenBy(user => user.FullName)
            .ThenBy(user => user.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Users
            .AsNoTracking()
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var context = DbContextFactory.CreateDbContext();

        user.CreatedAt = DateTime.Now;
        user.UpdatedAt = DateTime.Now;

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(user)
            .Reference(createdUser => createdUser.Role)
            .LoadAsync(cancellationToken);

        return user;
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        await using var context = DbContextFactory.CreateDbContext();

        var existingUser = await context.Users
            .FirstOrDefaultAsync(existing => existing.UserId == user.UserId, cancellationToken);

        if (existingUser is null)
        {
            return null;
        }

        existingUser.RoleId = user.RoleId;
        existingUser.Username = user.Username;
        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.Status = user.Status;
        existingUser.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .Include(updatedUser => updatedUser.Role)
            .FirstOrDefaultAsync(updatedUser => updatedUser.UserId == user.UserId, cancellationToken);
    }

    public async Task<User?> UpdateStatusAsync(
        int userId,
        UserStatus status,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var user = await context.Users
            .FirstOrDefaultAsync(existingUser => existingUser.UserId == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.Status = status;
        user.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync(cancellationToken);

        return await context.Users
            .AsNoTracking()
            .Include(updatedUser => updatedUser.Role)
            .FirstOrDefaultAsync(updatedUser => updatedUser.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(
        string username,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        var normalizedUsername = username.Trim();

        await using var context = DbContextFactory.CreateDbContext();

        return await context.Users.AnyAsync(
            user => user.Username == normalizedUsername
                && (!excludedUserId.HasValue || user.UserId != excludedUserId.Value),
            cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var normalizedEmail = email.Trim();

        await using var context = DbContextFactory.CreateDbContext();

        return await context.Users.AnyAsync(
            user => user.Email == normalizedEmail
                && (!excludedUserId.HasValue || user.UserId != excludedUserId.Value),
            cancellationToken);
    }

    public async Task<bool> RoleExistsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Roles.AnyAsync(role => role.RoleId == roleId, cancellationToken);
    }

    public async Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Roles
            .AsNoTracking()
            .OrderBy(role => role.RoleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasBusinessReferencesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var hasBookings = await context.Bookings
            .AnyAsync(booking => booking.CreatedByUserId == userId, cancellationToken);

        if (hasBookings)
        {
            return true;
        }

        var hasCheckRecords = await context.CheckRecords
            .AnyAsync(
                record => record.CheckInByUserId == userId || record.CheckOutByUserId == userId,
                cancellationToken);

        if (hasCheckRecords)
        {
            return true;
        }

        var hasServiceOrders = await context.ServiceOrders
            .AnyAsync(order => order.CreatedByUserId == userId, cancellationToken);

        if (hasServiceOrders)
        {
            return true;
        }

        var hasInvoices = await context.Invoices
            .AnyAsync(invoice => invoice.CreatedByUserId == userId, cancellationToken);

        if (hasInvoices)
        {
            return true;
        }

        return await context.Payments
            .AnyAsync(payment => payment.ReceivedByUserId == userId, cancellationToken);
    }
}
