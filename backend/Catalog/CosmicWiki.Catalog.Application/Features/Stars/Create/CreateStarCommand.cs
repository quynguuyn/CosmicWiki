using CosmicWiki.Catalog.Domain.ValueObjects;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Create;

public sealed record CreateStarCommand(
    string? Name,
    IReadOnlyCollection<CreateStarCatalogIdentifier> CatalogIdentifiers,
    IReadOnlyCollection<CreateStarParallax> Parallaxes
) : IRequest<Guid>;

public sealed record CreateStarCatalogIdentifier(
    string Catalog,
    string Value);

public sealed record CreateStarParallax(
    double ValueMas,
    MeasurementQuality Quality);