using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Auth.Domain.Entities;

public sealed class Role : AggregateRoot<Guid>
{
    private readonly List<RolePermission> _permissions = [];

    private Role(
        Guid id,
        string name,
        string? description)
        : base(id)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions =>
        _permissions.AsReadOnly();

    public static Role Create(
        string name,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim());
    }

    public void Update(
        string name,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = description?.Trim();
    }

    public void AssignPermission(Guid permissionId)
    {
        if (permissionId == Guid.Empty)
            throw new ArgumentException(
                "Permission ID cannot be empty.",
                nameof(permissionId));

        if (_permissions.Any(x => x.PermissionId == permissionId))
            return;

        _permissions.Add(
            RolePermission.Create(
                Id,
                permissionId));
    }

    public void RemovePermission(Guid permissionId)
    {
        if (permissionId == Guid.Empty)
            throw new ArgumentException(
                "Permission ID cannot be empty.",
                nameof(permissionId));

        _permissions.RemoveAll(
            x => x.PermissionId == permissionId);
    }
}