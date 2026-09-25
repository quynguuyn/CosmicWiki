using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Patch;

public sealed record PatchAstronomicalImageCommand(
    Guid AstronomicalObjectId,
    Guid ImageId,
    string? Url,
    IReadOnlyCollection<string>? Urls
) : IRequest;