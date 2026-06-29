using BusinessObjects.Entities;
using BusinessObjects.Enums;

namespace Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken = default);

    Task<Role?> GetByNameAsync(RoleName roleName, CancellationToken cancellationToken = default);
}
