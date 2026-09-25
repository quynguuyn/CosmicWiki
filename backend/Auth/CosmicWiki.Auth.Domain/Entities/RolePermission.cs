using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Auth.Domain.Entities;

public sealed class RolePermission : Entity<Guid>
{
    private RolePermission(
        Guid id,
        Guid roleId,
        Guid permissionId)
        : base(id)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public static RolePermission Create(
        Guid roleId,
        Guid permissionId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException(
                "Role ID cannot be empty.",
                nameof(roleId));

        if (permissionId == Guid.Empty)
            throw new ArgumentException(
                "Permission ID cannot be empty.",
                nameof(permissionId));

        return new RolePermission(
            Guid.NewGuid(),
            roleId,
            permissionId);
    }
}