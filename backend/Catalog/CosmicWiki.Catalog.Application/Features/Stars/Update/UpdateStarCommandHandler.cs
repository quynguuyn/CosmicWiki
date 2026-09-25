using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.ValueObjects;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Update;

public sealed class UpdateStarCommandHandler
    : IRequestHandler<UpdateStarCommand>
{
    private readonly IStarRepository _starRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IStarCache _starCache;

    public UpdateStarCommandHandler(
        IStarRepository starRepository,
        ICatalogUnitOfWork unitOfWork,
        IStarCache starCache)
    {
        _starRepository = starRepository;
        _unitOfWork = unitOfWork;
        _starCache = starCache;
    }

    public async Task Handle(
        UpdateStarCommand request,
        CancellationToken cancellationToken)
    {
        var star = await _starRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (star is null)
        {
            throw new KeyNotFoundException(
                $"Star '{request.Id}' was not found.");
        }

        var duplicateIdentifier = request.CatalogIdentifiers
            .GroupBy(x =>
                $"{x.Catalog.Trim().ToUpperInvariant()}:{x.Value.Trim().ToUpperInvariant()}")
            .FirstOrDefault(x => x.Count() > 1);

        if (duplicateIdentifier is not null)
        {
            throw new InvalidOperationException(
                $"Duplicate catalog identifier '{duplicateIdentifier.Key}'.");
        }

        foreach (var identifier in request.CatalogIdentifiers)
        {
            var existingStar =
                await _starRepository.GetByCatalogIdentifierAsync(
                    identifier.Catalog,
                    identifier.Value,
                    cancellationToken);

            if (existingStar is not null &&
                existingStar.Id != request.Id)
            {
                throw new InvalidOperationException(
                    $"Catalog identifier '{identifier.Catalog}:{identifier.Value}' already belongs to another star.");
            }
        }

        star.Rename(request.Name);

        star.ReplaceCatalogIdentifiers(
            request.CatalogIdentifiers.Select(identifier =>
                CatalogIdentifier.Create(
                    identifier.Catalog,
                    identifier.Value)));

        star.ReplaceParallaxes(
            request.Parallaxes.Select(parallax =>
                ParallaxMeasurement.Create(
                    parallax.ValueMas,
                    quality: parallax.Quality)));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _starCache.InvalidateAsync(request.Id, cancellationToken);
    }
}