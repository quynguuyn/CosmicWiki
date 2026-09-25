using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Product.Domain.Entities;

public sealed class Category : AggregateRoot<Guid>
{
    private Category() : base(Guid.Empty)
    {
    }
    
    private Category(
        Guid id,
        string name,
        string slug,
        string? description)
        : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
    }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public string? Description { get; private set; }

    public static Category Create(
        string name,
        string slug,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Category(
            Guid.NewGuid(),
            name.Trim(),
            slug.Trim(),
            description?.Trim());
    }

    public void Update(
        string name,
        string slug,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name.Trim();
        Slug = slug.Trim();
        Description = description?.Trim();
    }
}