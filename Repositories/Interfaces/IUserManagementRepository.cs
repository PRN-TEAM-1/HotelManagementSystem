using BusinessObjects.Entities;
using BusinessObjects.Enums;

namespace Repositories.Interfaces;

public interface IUserManagementRepository
{
    Task<List<User>> GetUsersAsync(string? searchTerm = null, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> UpdateStatusAsync(int userId, UserStatus status, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(
        string username,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        int? excludedUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> RoleExistsAsync(int roleId, CancellationToken cancellationToken = default);

    Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default);

    Task<bool> HasBusinessReferencesAsync(int userId, CancellationToken cancellationToken = default);
}
