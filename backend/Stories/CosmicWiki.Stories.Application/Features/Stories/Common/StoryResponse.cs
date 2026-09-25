using CosmicWiki.Stories.Domain.Enums;

namespace CosmicWiki.Stories.Application.Features.Stories.Common;

public sealed record StoryResponse(
    Guid Id,
    string Category,
    string Era,
    string Code,
    string Title,
    string Slug,
    string Description,
    string Content,
    string Author,
    int ReadTimeMinutes,
    string? ImageUrl,
    string? ImageAlt,
    StoryStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
