using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Auth.Domain.Entities;

public sealed class UserRole : Entity<Guid>
{
    private UserRole(
        Guid id,
        Guid userId,
        Guid roleId)
        : base(id)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public static UserRole Create(
        Guid userId,
        Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.",
                nameof(userId));

        if (roleId == Guid.Empty)
            throw new ArgumentException(
                "Role ID cannot be empty.",
                nameof(roleId));

        return new UserRole(
            Guid.NewGuid(),
            userId,
            roleId);
    }
}