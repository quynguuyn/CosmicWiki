using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;

namespace CosmicWiki.Catalog.Application.Abstractions.Persistence;

public interface IAstronomicalObjectRepository
{
    Task<AstronomicalObject?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AstronomicalObjectReadModel?> GetReadModelByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AstronomicalObjectImageTarget>> GetImageTargetsByCatalogValuesAsync(
        string catalog,
        IReadOnlyCollection<string> catalogValues,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        AstronomicalObject astronomicalObject,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AstronomicalObjectReadModel> Items, int TotalCount)> GetPagedAsync(
        int skip,
        int pageSize,
        AstronomicalObjectKind? kind = null,
        CancellationToken cancellationToken = default);
    
    void Delete(AstronomicalObject astronomicalObject);
}

public sealed record AstronomicalObjectReadModel(
    Guid Id,
    string? Name,
    AstronomicalObjectKind Kind,
    string? ObjectType,
    string? Catalog,
    string? CatalogValue,
    double? RightAscensionDegrees,
    double? DeclinationDegrees,
    string? ImageUrl);

public sealed record AstronomicalObjectImageTarget(
    Guid Id,
    AstronomicalObjectKind Kind,
    string? CatalogValue,
    double? RightAscensionDegrees,
    double? DeclinationDegrees);