using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Domain.Enums;
using CosmicWiki.Catalog.Infrastructure.Research;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Catalog;

namespace CosmicWiki.Product.UnitTests.Catalog.Research;

public sealed class AstronomicalResearchCandidateProcessorTests
{
    [Fact]
    public async Task ProcessPageAsync_PersistsNewStarAndPublishesOutboxEvent()
    {
        var starRepository = Substitute.For<IStarRepository>();
        var objectRepository = Substitute.For<IAstronomicalObjectRepository>();
        var imageEnricher = Substitute.For<IAstronomicalResearchImageEnricher>();
        var unitOfWork = Substitute.For<ICatalogUnitOfWork>();
        var outboxWriter = Substitute.For<IOutboxWriter>();
        starRepository.GetExistingCatalogIdentifierValuesAsync(
                "SIMBAD",
                Arg.Any<IReadOnlyCollection<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>()));
        objectRepository.GetImageTargetsByCatalogValuesAsync(
                "SIMBAD",
                Arg.Any<IReadOnlyCollection<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<AstronomicalObjectImageTarget>>(
                Array.Empty<AstronomicalObjectImageTarget>()));
        imageEnricher.EnsureSurveyImagesAsync(
                Arg.Any<Guid>(),
                Arg.Any<AstronomicalResearchCandidate>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(string? MainUrl, bool Changed)>(("https://image", true)));
        var processor = new AstronomicalResearchCandidateProcessor(
            starRepository,
            objectRepository,
            imageEnricher,
            unitOfWork,
            outboxWriter);
        var candidate = new AstronomicalResearchCandidate(
            "Vega",
            AstronomicalObjectKind.Star,
            "A0V",
            "SIMBAD",
            "Vega",
            279.2347,
            38.7837,
            130.23);

        var result = await processor.ProcessPageAsync(
            [candidate],
            CancellationToken.None);

        Assert.Equal(new AstronomicalResearchPageResult(1, 0), result);
        await starRepository.Received(1).AddAsync(
            Arg.Is<CosmicWiki.Catalog.Domain.Entities.Star>(star =>
                star.CatalogIdentifiers.Any(identifier =>
                    identifier.Value == candidate.CatalogValue)),
            CancellationToken.None);
        await objectRepository.Received(1).AddAsync(
            Arg.Any<CosmicWiki.Catalog.Domain.Entities.AstronomicalObject>(),
            CancellationToken.None);
        outboxWriter.Received(1).Add(
            Arg.Any<StarCreatedIntegrationEvent>(),
            KafkaTopics.CatalogStarCreated);
        await unitOfWork.Received(1).SaveChangesAsync(CancellationToken.None);
    }
}
