using CosmicWiki.SharedKernel.Abstractions;
using CosmicWiki.Stories.Domain.Enums;

namespace CosmicWiki.Stories.Domain.Entities;

public sealed class Story : AggregateRoot<Guid>
{
    private Story() : base(Guid.Empty)
    {
    }

    private Story(
        Guid id,
        string category,
        string era,
        string code,
        string title,
        string slug,
        string description,
        string content,
        string author,
        int readTimeMinutes,
        string? imageUrl,
        string? imageAlt,
        StoryStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
        : base(id)
    {
        Category = category;
        Era = era;
        Code = code;
        Title = title;
        Slug = slug;
        Description = description;
        Content = content;
        Author = author;
        ReadTimeMinutes = readTimeMinutes;
        ImageUrl = imageUrl;
        ImageAlt = imageAlt;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public string Category { get; private set; } = null!;
    public string Era { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string Author { get; private set; } = null!;
    public int ReadTimeMinutes { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ImageAlt { get; private set; }
    public StoryStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Story Create(
        string category,
        string era,
        string code,
        string title,
        string slug,
        string description,
        string content,
        string author,
        int readTimeMinutes,
        string? imageUrl = null,
        string? imageAlt = null,
        StoryStatus status = StoryStatus.Draft)
    {
        Validate(category, era, code, title, slug, description, content, author, readTimeMinutes);
        var now = DateTime.UtcNow;
        return new Story(
            Guid.NewGuid(),
            category.Trim(),
            era.Trim(),
            code.Trim(),
            title.Trim(),
            slug.Trim(),
            description.Trim(),
            content.Trim(),
            author.Trim(),
            readTimeMinutes,
            imageUrl?.Trim(),
            imageAlt?.Trim(),
            status,
            now,
            now);
    }

    public void Update(
        string category,
        string era,
        string code,
        string title,
        string slug,
        string description,
        string content,
        string author,
        int readTimeMinutes,
        string? imageUrl,
        string? imageAlt,
        StoryStatus status)
    {
        Validate(category, era, code, title, slug, description, content, author, readTimeMinutes);
        Category = category.Trim();
        Era = era.Trim();
        Code = code.Trim();
        Title = title.Trim();
        Slug = slug.Trim();
        Description = description.Trim();
        Content = content.Trim();
        Author = author.Trim();
        ReadTimeMinutes = readTimeMinutes;
        ImageUrl = imageUrl?.Trim();
        ImageAlt = imageAlt?.Trim();
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void Validate(
        string category,
        string era,
        string code,
        string title,
        string slug,
        string description,
        string content,
        string author,
        int readTimeMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(era);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(author);
        if (readTimeMinutes < 1)
            throw new ArgumentOutOfRangeException(nameof(readTimeMinutes));
    }
}
