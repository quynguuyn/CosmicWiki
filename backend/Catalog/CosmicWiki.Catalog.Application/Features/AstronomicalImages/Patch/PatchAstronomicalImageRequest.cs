using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Patch;

public sealed record PatchAstronomicalImageRequest(
    string? Url,
    IReadOnlyCollection<string>? Urls
) : IRequest;