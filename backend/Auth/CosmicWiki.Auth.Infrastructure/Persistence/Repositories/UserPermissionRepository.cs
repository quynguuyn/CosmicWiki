using CosmicWiki.Auth.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Repositories;

public sealed class UserPermissionRepository
    : IUserPermissionRepository
{
    private readonly AuthDbContext _dbContext;

    public UserPermissionRepository(
        AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await (
                from userRole in _dbContext.UserRoles
                join rolePermission in _dbContext.RolePermissions
                    on userRole.RoleId equals rolePermission.RoleId
                join permission in _dbContext.Permissions
                    on rolePermission.PermissionId equals permission.Id
                where userRole.UserId == userId
                select permission.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }
}