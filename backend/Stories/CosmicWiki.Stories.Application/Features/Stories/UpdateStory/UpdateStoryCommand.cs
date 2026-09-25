using CosmicWiki.Stories.Domain.Enums;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.UpdateStory;

public sealed record UpdateStoryCommand(
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
    StoryStatus Status) : IRequest;

public sealed record UpdateStoryRequest(
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
    StoryStatus Status);
