using CosmicWiki.Stories.Domain.Enums;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.PatchStory;

public sealed record PatchStoryCommand(
    Guid Id,
    string? Category,
    string? Era,
    string? Code,
    string? Title,
    string? Slug,
    string? Description,
    string? Content,
    string? Author,
    int? ReadTimeMinutes,
    string? ImageUrl,
    string? ImageAlt,
    StoryStatus? Status) : IRequest;

public sealed record PatchStoryRequest(
    string? Category,
    string? Era,
    string? Code,
    string? Title,
    string? Slug,
    string? Description,
    string? Content,
    string? Author,
    int? ReadTimeMinutes,
    string? ImageUrl,
    string? ImageAlt,
    StoryStatus? Status);
