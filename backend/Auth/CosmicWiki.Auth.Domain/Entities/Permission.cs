using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Auth.Domain.Entities;

public sealed class Permission : Entity<Guid>
{
    private Permission(
        Guid id,
        string code,
        string name,
        string? description)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public static Permission Create(
        string code,
        string name,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Permission(
            Guid.NewGuid(),
            code.Trim(),
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
}