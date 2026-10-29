using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Catalog;
using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;
using CosmicWiki.Catalog.Domain.ValueObjects;
using CosmicWiki.Caching;
using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Create;

public sealed class CreateStarCommandHandler
    : IRequestHandler<CreateStarCommand, Guid>
{
    private readonly IStarRepository _starRepository;
    private readonly IAstronomicalObjectRepository _astronomicalObjectRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IStarCache _starCache;

    public CreateStarCommandHandler(
        IStarRepository starRepository,
        IAstronomicalObjectRepository astronomicalObjectRepository,
        ICatalogUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IStarCache starCache)
    {
        _starRepository = starRepository;
        _astronomicalObjectRepository = astronomicalObjectRepository;
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
        _starCache = starCache;
    }

    public async Task<Guid> Handle(
        CreateStarCommand request,
        CancellationToken cancellationToken)
    {
        var duplicateIdentifier = request.CatalogIdentifiers
            .GroupBy(x => $"{x.Catalog.Trim().ToUpperInvariant()}:{x.Value.Trim().ToUpperInvariant()}")
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

            if (existingStar is not null)
            {
                throw new InvalidOperationException(
                    $"Catalog identifier '{identifier.Catalog}:{identifier.Value}' already exists.");
            }
        }

        var primaryIdentifier = request.CatalogIdentifiers.FirstOrDefault();
        var astronomicalObject = AstronomicalObject.Create(
            request.Name,
            AstronomicalObjectKind.Star,
            null,
            primaryIdentifier?.Catalog,
            primaryIdentifier?.Value);

        var star = Star.Create(
            astronomicalObject.Id,
            request.Name);

        foreach (var identifier in request.CatalogIdentifiers)
        {
            star.AddCatalogIdentifier(
                CatalogIdentifier.Create(
                    identifier.Catalog,
                    identifier.Value));
        }

        foreach (var parallax in request.Parallaxes)
        {
            star.AddParallax(
                ParallaxMeasurement.Create(
                    parallax.ValueMas,
                    quality: parallax.Quality));
        }

        await _astronomicalObjectRepository.AddAsync(
            astronomicalObject,
            cancellationToken);

        await _starRepository.AddAsync(
            star,
            cancellationToken);

        _outboxWriter.Add(new StarCreatedIntegrationEvent(
            Guid.NewGuid(), 1, DateTime.UtcNow, star.Id, star.Id,
            star.Name ?? throw new InvalidOperationException("A star name is required for search indexing."),
            null, null,
            primaryIdentifier?.Catalog,
            primaryIdentifier?.Value),
            KafkaTopics.CatalogStarCreated);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _starCache.InvalidateAsync(star.Id, cancellationToken);

        return star.Id;
    }
}
