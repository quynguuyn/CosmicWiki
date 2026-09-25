using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Visit.Domain.Entities;

public sealed class Exhibition : AggregateRoot<Guid>
{
    private Exhibition(
        Guid id,
        string number,
        string category,
        string title,
        string description,
        string status,
        string? imageUrl,
        int displayOrder,
        bool isPublished) : base(id)
    {
        Number = number;
        Category = category;
        Title = title;
        Description = description;
        Status = status;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    private Exhibition() : base(Guid.Empty) { }

    public string Number { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void Update(
        string number,
        string category,
        string title,
        string description,
        string status,
        string? imageUrl,
        int displayOrder,
        bool isPublished)
    {
        Number = number.Trim();
        Category = category.Trim();
        Title = title.Trim();
        Description = description.Trim();
        Status = status.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Exhibition Create(
        string number,
        string category,
        string title,
        string description,
        string status,
        string? imageUrl,
        int displayOrder,
        bool isPublished) =>
        new(
            Guid.NewGuid(),
            number.Trim(),
            category.Trim(),
            title.Trim(),
            description.Trim(),
            status.Trim(),
            string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim(),
            displayOrder,
            isPublished);
}
