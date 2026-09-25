namespace CosmicWiki.Discovery.Application.Features.Search;

public sealed record SearchResultResponse(
    Guid Id,
    Guid SourceId,
    string SourceType,
    string Name,
    string? Description,
    string? ImageUrl);