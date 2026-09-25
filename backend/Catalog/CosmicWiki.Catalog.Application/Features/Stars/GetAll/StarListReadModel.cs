namespace CosmicWiki.Catalog.Application.Features.Stars.GetAll;

public sealed record StarListReadModel(
    Guid Id,
    string? Name,
    IReadOnlyCollection<CatalogIdentifierReadModel>
        CatalogIdentifiers);

public sealed record CatalogIdentifierReadModel(
    string Catalog,
    string Value);