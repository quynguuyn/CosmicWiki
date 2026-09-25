using CosmicWiki.Auth.Domain.DomainEvents;
using CosmicWiki.Auth.Domain.Enums;
using CosmicWiki.Auth.Domain.ValueObjects;
using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Auth.Domain.Entities;

public sealed class User : AggregateRoot<Guid>
{
    private readonly List<UserRole> _roles = [];

    private User(
        Guid id,
        Username username,
        Email email,
        string passwordHash)
        : base(id)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Status = UserStatus.Active;

        CreatedAt = DateTime.UtcNow;
    }

    public Username Username { get; private set; }

    public Email Email { get; private set; }

    public string PasswordHash { get; private set; }

    public UserStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<UserRole> Roles =>
        _roles.AsReadOnly();

    public static User Create(
        Username username,
        Email email,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(
                "Password hash cannot be empty.",
                nameof(passwordHash));

        var user = new User(
            Guid.NewGuid(),
            username,
            email,
            passwordHash);

        user.AddDomainEvent(
            new UserRegisteredDomainEvent(
                user.Id,
                user.Username.Value,
                user.CreatedAt));

        return user;
    }

    public void ChangeEmail(Email email)
    {
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException(
                "Password hash cannot be empty.",
                nameof(passwordHash));

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Lock()
    {
        Status = UserStatus.Locked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException(
                "Role ID cannot be empty.",
                nameof(roleId));

        if (_roles.Any(x => x.RoleId == roleId))
            return;

        _roles.Add(
            UserRole.Create(
                Id,
                roleId));

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException(
                "Role ID cannot be empty.",
                nameof(roleId));

        _roles.RemoveAll(
            x => x.RoleId == roleId);

        UpdatedAt = DateTime.UtcNow;
    }
}