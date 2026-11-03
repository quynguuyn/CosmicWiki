using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Domain.Enums;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Catalog;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class AstronomicalResearchCandidateProcessor(
    IStarRepository starRepository,
    IAstronomicalObjectRepository objectRepository,
    IAstronomicalResearchImageEnricher imageEnricher,
    ICatalogUnitOfWork unitOfWork,
    IOutboxWriter outboxWriter) : IAstronomicalResearchCandidateProcessor
{
    public async Task<AstronomicalResearchPageResult> ProcessPageAsync(
        IReadOnlyCollection<AstronomicalResearchCandidate> candidates,
        CancellationToken cancellationToken)
    {
        var catalogValues = candidates.Select(candidate => candidate.CatalogValue).ToArray();
        var existingValues = await starRepository.GetExistingCatalogIdentifierValuesAsync(
            "SIMBAD",
            catalogValues,
            cancellationToken);
        var existingSet = existingValues.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existingObjects = await objectRepository.GetImageTargetsByCatalogValuesAsync(
            "SIMBAD",
            catalogValues,
            cancellationToken);
        var existingObjectByCatalogValue = existingObjects
            .Where(item => item.CatalogValue is not null)
            .ToDictionary(item => item.CatalogValue!, StringComparer.OrdinalIgnoreCase);
        var pageChanged = false;
        var createdCount = 0;
        var existingCount = 0;

        foreach (var candidate in candidates)
        {
            existingObjectByCatalogValue.TryGetValue(
                candidate.CatalogValue,
                out var existingObject);
            var existingStar = candidate.Kind == AstronomicalObjectKind.Star &&
                               existingSet.Contains(candidate.CatalogValue)
                ? await starRepository.GetByCatalogIdentifierAsync(
                    candidate.Catalog,
                    candidate.CatalogValue,
                    cancellationToken)
                : null;

            var objectCreated = existingObject is null;
            var astronomicalObjectId = existingObject?.Id ??
                                       existingStar?.Id ??
                                       Guid.NewGuid();
            if (objectCreated)
            {
                await objectRepository.AddAsync(
                    AstronomicalResearchCandidateMapper.CreateAstronomicalObject(
                        astronomicalObjectId,
                        candidate),
                    cancellationToken);
            }

            var starCreated = candidate.Kind == AstronomicalObjectKind.Star &&
                              existingStar is null;
            if (starCreated)
            {
                await starRepository.AddAsync(
                    AstronomicalResearchCandidateMapper.CreateStar(
                        astronomicalObjectId,
                        candidate),
                    cancellationToken);
            }

            var imageCandidate = candidate with
            {
                RightAscensionDegrees =
                    candidate.RightAscensionDegrees ??
                    existingObject?.RightAscensionDegrees,
                DeclinationDegrees =
                    candidate.DeclinationDegrees ??
                    existingObject?.DeclinationDegrees
            };
            var (imageUrl, imageChanged) =
                await imageEnricher.EnsureSurveyImagesAsync(
                    astronomicalObjectId,
                    imageCandidate,
                    cancellationToken);

            if (candidate.Kind == AstronomicalObjectKind.Star)
            {
                if (starCreated || imageChanged)
                {
                    outboxWriter.Add(
                        new StarCreatedIntegrationEvent(
                            Guid.NewGuid(),
                            1,
                            DateTime.UtcNow,
                            astronomicalObjectId,
                            astronomicalObjectId,
                            candidate.Name,
                            null,
                            imageUrl,
                            candidate.Catalog,
                            candidate.CatalogValue),
                        KafkaTopics.CatalogStarCreated);
                }
            }
            else if (objectCreated || imageChanged)
            {
                outboxWriter.Add(
                    new AstronomicalObjectCreatedIntegrationEvent(
                        Guid.NewGuid(),
                        1,
                        DateTime.UtcNow,
                        astronomicalObjectId,
                        astronomicalObjectId,
                        candidate.Kind.ToString(),
                        candidate.Name,
                        candidate.ObjectType,
                        imageUrl,
                        candidate.Catalog,
                        candidate.CatalogValue),
                    KafkaTopics.CatalogAstronomicalObjectCreated);
            }

            if (objectCreated || starCreated || imageChanged)
            {
                pageChanged = true;
                if (objectCreated || starCreated)
                    createdCount++;
            }
            else
            {
                existingCount++;
            }
        }

        if (pageChanged)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AstronomicalResearchPageResult(createdCount, existingCount);
    }
}
