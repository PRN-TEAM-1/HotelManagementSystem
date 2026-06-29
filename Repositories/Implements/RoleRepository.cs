using BusinessObjects.Entities;
using BusinessObjects.Enums;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class RoleRepository : IRoleRepository
{
    private readonly RoleDao _roleDao;

    public RoleRepository(RoleDao? roleDao = null)
    {
        _roleDao = roleDao ?? new RoleDao();
    }

    public Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return _roleDao.GetByIdAsync(roleId, cancellationToken);
    }

    public Task<Role?> GetByNameAsync(RoleName roleName, CancellationToken cancellationToken = default)
    {
        return _roleDao.GetByNameAsync(roleName, cancellationToken);
    }
}
