using CosmicWiki.Stories.Domain.Enums;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.CreateStory;

public sealed record CreateStoryCommand(
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
    StoryStatus Status = StoryStatus.Draft) : IRequest<Guid>;
