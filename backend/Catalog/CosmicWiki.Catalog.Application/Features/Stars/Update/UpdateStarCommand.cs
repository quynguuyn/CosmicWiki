using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Update;

public sealed record UpdateStarCommand(
    Guid Id,
    string? Name,
    IReadOnlyCollection<UpdateStarCatalogIdentifier> CatalogIdentifiers,
    IReadOnlyCollection<UpdateStarParallax> Parallaxes
) : IRequest;