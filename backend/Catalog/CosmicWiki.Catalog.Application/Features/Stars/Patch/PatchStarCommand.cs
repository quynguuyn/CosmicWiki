using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Patch;

public sealed record PatchStarCommand(
    Guid Id,
    string? Name,
    IReadOnlyCollection<PatchStarCatalogIdentifier>? CatalogIdentifiers,
    IReadOnlyCollection<PatchStarParallax>? Parallaxes
) : IRequest;