using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Auth.Infrastructure.Persistence.Repositories;

public sealed class AdminAccountRepository(AuthDbContext db)
    : IAdminAccountRepository
{
    public async Task<(IReadOnlyCollection<AdminAccountProjection> Items, int TotalCount)> GetAccountsAsync(
        string? role,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(role)
            ? db.Users.FromSqlInterpolated($@"
                SELECT u.*
                FROM USERS u
                WHERE EXISTS (
                    SELECT 1
                    FROM USER_ROLES ur
                    JOIN ROLES r ON r.ID = ur.ROLE_ID
                    WHERE ur.USER_ID = u.ID
                      AND r.NAME <> {"User"})")
            : db.Users.FromSqlInterpolated($@"
                SELECT u.*
                FROM USERS u
                WHERE EXISTS (
                    SELECT 1
                    FROM USER_ROLES ur
                    JOIN ROLES r ON r.ID = ur.ROLE_ID
                    WHERE ur.USER_ID = u.ID
                      AND r.NAME = {role})");
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(user => user.Username)
            .ThenBy(user => user.Id)
            .Skip(skip)
            .Take(take)
            .Select(user => new
            {
                user.Id,
                Username = user.Username.Value,
                Email = user.Email.Value,
                Status = user.Status.ToString(),
                user.CreatedAt,
                Roles = user.Roles
                    .Join(db.Roles, userRole => userRole.RoleId, assignedRole => assignedRole.Id,
                        (_, assignedRole) => assignedRole.Name)
                    .Distinct()
            })
            .ToListAsync(cancellationToken);

        var items = users.Select(user => new AdminAccountProjection(
            user.Id, user.Username, user.Email, user.Status, user.CreatedAt,
            user.Roles.ToArray())).ToArray();
        return (items, totalCount);
    }

    public async Task<IReadOnlyCollection<RoleProjection>> GetRolesAsync(
        CancellationToken cancellationToken) =>
        await db.Roles.OrderBy(role => role.Name)
            .Select(role => new RoleProjection(role.Id, role.Name, role.Description))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PermissionProjection>> GetPermissionsAsync(
        CancellationToken cancellationToken) =>
        await db.Permissions.OrderBy(permission => permission.Code)
            .Select(permission => new PermissionProjection(
                permission.Id, permission.Code, permission.Name, permission.Description))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PermissionProjection>?> GetRolePermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        if (!await db.Roles.AnyAsync(role => role.Id == roleId, cancellationToken))
            return null;

        return await db.RolePermissions.Where(link => link.RoleId == roleId)
            .Join(db.Permissions, link => link.PermissionId, permission => permission.Id,
                (_, permission) => new PermissionProjection(
                    permission.Id, permission.Code, permission.Name, permission.Description))
            .OrderBy(permission => permission.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AssignPermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var role = await db.Roles.SingleOrDefaultAsync(
            item => item.Id == roleId, cancellationToken);
        if (role is null || !await db.Permissions.AnyAsync(
                permission => permission.Id == permissionId, cancellationToken))
            return false;

        role.AssignPermission(permissionId);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemovePermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var link = await db.RolePermissions.SingleOrDefaultAsync(
            item => item.RoleId == roleId && item.PermissionId == permissionId,
            cancellationToken);
        if (link is null)
            return false;

        db.RolePermissions.Remove(link);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
