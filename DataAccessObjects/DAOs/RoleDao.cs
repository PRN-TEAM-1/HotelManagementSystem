using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class RoleDao
{
    public async Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.RoleId == roleId, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(RoleName roleName, CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(role => role.Name == roleName, cancellationToken);
    }
}
