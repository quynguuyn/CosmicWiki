using CosmicWiki.Catalog.Domain.ValueObjects;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Update;

public sealed record UpdateStarRequest(
    string? Name,
    IReadOnlyCollection<UpdateStarCatalogIdentifier> CatalogIdentifiers,
    IReadOnlyCollection<UpdateStarParallax> Parallaxes
) : IRequest;

public sealed record UpdateStarCatalogIdentifier(
    string Catalog,
    string Value);

public sealed record UpdateStarParallax(
    double ValueMas,
    MeasurementQuality Quality);