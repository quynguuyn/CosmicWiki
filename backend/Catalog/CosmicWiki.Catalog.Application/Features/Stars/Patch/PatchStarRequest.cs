using CosmicWiki.Catalog.Domain.ValueObjects;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Patch;

public sealed record PatchStarRequest(
    string? Name,
    IReadOnlyCollection<PatchStarCatalogIdentifier>? CatalogIdentifiers,
    IReadOnlyCollection<PatchStarParallax>? Parallaxes
) : IRequest;

public sealed record PatchStarCatalogIdentifier(
    string Catalog,
    string Value);

public sealed record PatchStarParallax(
    double ValueMas,
    MeasurementQuality Quality);