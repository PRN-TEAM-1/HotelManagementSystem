using BusinessObjects.Entities;
using BusinessObjects.Enums;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class UserManagementRepository : IUserManagementRepository
{
    private readonly UserManagementDao _dao;

    public UserManagementRepository(UserManagementDao? dao = null)
    {
        _dao = dao ?? new UserManagementDao();
    }

    public Task<List<User>> GetUsersAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        return _dao.GetUsersAsync(searchTerm, cancellationToken);
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dao.GetByIdAsync(userId, cancellationToken);
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        return _dao.AddAsync(user, cancellationToken);
    }

    public Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        return _dao.UpdateAsync(user, cancellationToken);
    }

    public Task<User?> UpdateStatusAsync(
        int userId,
        UserStatus status,
        CancellationToken cancellationToken = default)
    {
        return _dao.UpdateStatusAsync(userId, status, cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(
        string username,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default)
    {
        return _dao.ExistsByUsernameAsync(username, excludedUserId, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(
        string email,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default)
    {
        return _dao.ExistsByEmailAsync(email, excludedUserId, cancellationToken);
    }

    public Task<bool> RoleExistsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return _dao.RoleExistsAsync(roleId, cancellationToken);
    }

    public Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        return _dao.GetRolesAsync(cancellationToken);
    }

    public Task<bool> HasBusinessReferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _dao.HasBusinessReferencesAsync(userId, cancellationToken);
    }
}
