namespace CosmicWiki.Discovery.Domain.Entities;

public sealed class SearchDocument
{
    private SearchDocument()
    {
    }

    public Guid Id { get; private set; }

    public Guid SourceId { get; private set; }

    public string SourceType { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? ImageUrl { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static SearchDocument Create(
        Guid sourceId,
        string sourceType,
        string name,
        string? description,
        string? imageUrl,
        DateTime updatedAt)
    {
        return new SearchDocument
        {
            Id = Guid.NewGuid(),
            SourceId = sourceId,
            SourceType = sourceType,
            Name = name,
            Description = description,
            ImageUrl = imageUrl,
            UpdatedAt = updatedAt
        };
    }

    public void Update(string name, string? description, string? imageUrl, DateTime updatedAt)
    {
        if (updatedAt <= UpdatedAt)
            return;

        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        UpdatedAt = updatedAt;
    }
}
