namespace CosmicWiki.Auth.Application.Abstractions.Persistence;

public interface IAdminAccountRepository
{
    Task<(IReadOnlyCollection<AdminAccountProjection> Items, int TotalCount)> GetAccountsAsync(
        string? role,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RoleProjection>> GetRolesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PermissionProjection>> GetPermissionsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PermissionProjection>?> GetRolePermissionsAsync(
        Guid roleId,
        CancellationToken cancellationToken);

    Task<bool> AssignPermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken);

    Task<bool> RemovePermissionAsync(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken);
}

public sealed record AdminAccountProjection(
    Guid UserId,
    string Username,
    string Email,
    string Status,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Roles);

public sealed record RoleProjection(
    Guid RoleId,
    string Name,
    string? Description);

public sealed record PermissionProjection(
    Guid PermissionId,
    string Code,
    string Name,
    string? Description);
