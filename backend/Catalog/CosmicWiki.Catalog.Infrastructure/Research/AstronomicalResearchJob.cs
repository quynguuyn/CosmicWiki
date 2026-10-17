using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;
using CosmicWiki.Catalog.Domain.ValueObjects;
using CosmicWiki.Caching;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Catalog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class AstronomicalResearchJob : IAstronomicalResearchJob
{
    private const string CheckpointCacheKey =
        "admin:jobs:astronomical-research:checkpoint";
    private const string LastRunCacheKey =
        "admin:jobs:astronomical-research:last-run";
    private static readonly SemaphoreSlim RunLock = new(1, 1);

    private readonly IAstronomicalResearchProvider _provider;
    private readonly IStarRepository _starRepository;
    private readonly IAstronomicalObjectRepository _objectRepository;
    private readonly IAstronomicalImageRepository _imageRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly AstronomicalResearchOptions _options;
    private readonly ILogger<AstronomicalResearchJob> _logger;
    private readonly ICacheStore _cacheStore;

    public AstronomicalResearchJob(
        IAstronomicalResearchProvider provider,
        IStarRepository starRepository,
        IAstronomicalObjectRepository objectRepository,
        IAstronomicalImageRepository imageRepository,
        ICatalogUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IOptions<AstronomicalResearchOptions> options,
        ILogger<AstronomicalResearchJob> logger,
        ICacheStore cacheStore)
    {
        _provider = provider;
        _starRepository = starRepository;
        _objectRepository = objectRepository;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
        _options = options.Value;
        _logger = logger;
        _cacheStore = cacheStore;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Astronomical research job is disabled.");
            return;
        }

        await RunLock.WaitAsync(cancellationToken);
        try
        {
            await RunBatchAsync(cancellationToken);
        }
        finally
        {
            RunLock.Release();
        }
    }

    private async Task RunBatchAsync(CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTime.UtcNow;
        var pageSize = Math.Clamp(_options.PageSize, 1, 500);
        var maxPages = Math.Clamp(_options.MaxPagesPerRun, 1, 100);
        var checkpoint = await _cacheStore.GetAsync<ResearchCheckpoint>(
            CheckpointCacheKey,
            cancellationToken);
        var cursor = checkpoint?.AfterMainId;
        var candidateCount = 0;
        var createdCount = 0;
        var existingCount = 0;
        var pagesProcessed = 0;
        var sweepCompleted = false;

        for (var pageNumber = 0; pageNumber < maxPages; pageNumber++)
        {
            var page = await _provider.FindCandidatesAsync(
                cursor,
                pageSize,
                cancellationToken);
            pagesProcessed++;
            candidateCount += page.Candidates.Count;

            var uniqueCandidates = page.Candidates
                .DistinctBy(
                    candidate =>
                        $"{candidate.Catalog}:{candidate.CatalogValue}",
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var existingValues =
                await _starRepository.GetExistingCatalogIdentifierValuesAsync(
                    "SIMBAD",
                    uniqueCandidates.Select(candidate => candidate.CatalogValue).ToArray(),
                    cancellationToken);
            var existingSet = existingValues.ToHashSet(
                StringComparer.OrdinalIgnoreCase);
            var existingObjects =
                await _objectRepository.GetImageTargetsByCatalogValuesAsync(
                    "SIMBAD",
                    uniqueCandidates.Select(candidate => candidate.CatalogValue).ToArray(),
                    cancellationToken);
            var existingObjectByCatalogValue = existingObjects
                .Where(item => item.CatalogValue is not null)
                .ToDictionary(
                    item => item.CatalogValue!,
                    StringComparer.OrdinalIgnoreCase);
            var pageChanged = false;

            foreach (var candidate in uniqueCandidates)
            {
                existingObjectByCatalogValue.TryGetValue(
                    candidate.CatalogValue,
                    out var existingObject);
                var existingStar = candidate.Kind == AstronomicalObjectKind.Star &&
                                   existingSet.Contains(candidate.CatalogValue)
                    ? await _starRepository.GetByCatalogIdentifierAsync(
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
                    var astronomicalObject = AstronomicalObject.Create(
                        astronomicalObjectId,
                        candidate.Name,
                        candidate.Kind,
                        candidate.ObjectType,
                        candidate.Catalog,
                        candidate.CatalogValue,
                        candidate.RightAscensionDegrees,
                        candidate.DeclinationDegrees);
                    await _objectRepository.AddAsync(
                        astronomicalObject,
                        cancellationToken);
                }

                var starCreated = candidate.Kind == AstronomicalObjectKind.Star &&
                                  existingStar is null;
                if (starCreated)
                {
                    var star = CreateStar(astronomicalObjectId, candidate);
                    await _starRepository.AddAsync(star, cancellationToken);
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
                var (imageUrl, imageChanged) = await EnsureSurveyImagesAsync(
                    astronomicalObjectId,
                    imageCandidate,
                    cancellationToken);

                if (candidate.Kind == AstronomicalObjectKind.Star)
                {
                    if (starCreated || imageChanged)
                    {
                        _outboxWriter.Add(
                            new StarCreatedIntegrationEvent(
                                Guid.NewGuid(),
                                1,
                                DateTime.UtcNow,
                                astronomicalObjectId,
                                astronomicalObjectId,
                                candidate.Name,
                                null,
                                imageUrl),
                            KafkaTopics.CatalogStarCreated);
                    }
                }
                else if (objectCreated || imageChanged)
                {
                    _outboxWriter.Add(
                        new AstronomicalObjectCreatedIntegrationEvent(
                            Guid.NewGuid(),
                            1,
                            DateTime.UtcNow,
                            astronomicalObjectId,
                            astronomicalObjectId,
                            candidate.Kind.ToString(),
                            candidate.Name,
                            candidate.ObjectType,
                            imageUrl),
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
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (page.HasMore && string.IsNullOrWhiteSpace(page.NextCursor))
                throw new InvalidOperationException(
                    "SIMBAD reported more results without a usable cursor.");

            cursor = page.HasMore ? page.NextCursor : null;
            if (page.HasMore)
            {
                await _cacheStore.SetAsync(
                    CheckpointCacheKey,
                    new ResearchCheckpoint(cursor),
                    TimeSpan.FromDays(365),
                    cancellationToken);
            }
            else
            {
                await _cacheStore.RemoveAsync(
                    CheckpointCacheKey,
                    cancellationToken);
                sweepCompleted = true;
                break;
            }
        }

        var result = new AstronomicalResearchRunResult(
            startedAtUtc,
            DateTime.UtcNow,
            candidateCount,
            existingCount,
            createdCount,
            pagesProcessed,
            sweepCompleted,
            cursor);

        await _cacheStore.SetAsync(
            LastRunCacheKey,
            result,
            TimeSpan.FromDays(7),
            cancellationToken);

        _logger.LogInformation(
            "Astronomical research batch completed. Pages: {PagesProcessed}, candidates: {CandidateCount}, existing: {ExistingCount}, created: {CreatedCount}, sweep completed: {SweepCompleted}, next cursor: {NextCursor}.",
            pagesProcessed,
            candidateCount,
            existingCount,
            createdCount,
            sweepCompleted,
            cursor);
    }

    private async Task<(string? MainUrl, bool Changed)> EnsureSurveyImagesAsync(
        Guid astronomicalObjectId,
        AstronomicalResearchCandidate candidate,
        CancellationToken cancellationToken)
    {
        if (!candidate.RightAscensionDegrees.HasValue ||
            !candidate.DeclinationDegrees.HasValue)
            return (null, false);

        var mainUrl = BuildSurveyImageUrl(
            "CDS/P/DSS2/color",
            candidate.RightAscensionDegrees.Value,
            candidate.DeclinationDegrees.Value);
        var alternateUrls = new[]
        {
            BuildSurveyImageUrl(
                "CDS/P/DSS2/red",
                candidate.RightAscensionDegrees.Value,
                candidate.DeclinationDegrees.Value),
            BuildSurveyImageUrl(
                "CDS/P/DSS2/blue",
                candidate.RightAscensionDegrees.Value,
                candidate.DeclinationDegrees.Value),
            BuildSurveyImageUrl(
                "CDS/P/DSS2/IR",
                candidate.RightAscensionDegrees.Value,
                candidate.DeclinationDegrees.Value),
            BuildSurveyImageUrl(
                "CDS/P/2MASS/color",
                candidate.RightAscensionDegrees.Value,
                candidate.DeclinationDegrees.Value),
            BuildSurveyImageUrl(
                "CDS/P/AllWISE/color",
                candidate.RightAscensionDegrees.Value,
                candidate.DeclinationDegrees.Value)
        };

        var images = await _imageRepository.GetByAstronomicalObjectIdAsync(
            astronomicalObjectId,
            cancellationToken);
        var image = images.FirstOrDefault();
        if (image is null)
        {
            await _imageRepository.AddAsync(
                AstronomicalImage.Create(
                    astronomicalObjectId,
                    mainUrl,
                    alternateUrls),
                cancellationToken);
            return (mainUrl, true);
        }

        if (image.Url != mainUrl ||
            !image.Urls.SequenceEqual(alternateUrls, StringComparer.Ordinal))
        {
            image.Update(mainUrl, alternateUrls);
            return (mainUrl, true);
        }

        return (mainUrl, false);
    }

    private static Star CreateStar(
        Guid id,
        AstronomicalResearchCandidate candidate)
    {
        var star = Star.Create(id, candidate.Name);
        star.AddCatalogIdentifier(
            CatalogIdentifier.Create(
                candidate.Catalog,
                candidate.CatalogValue));
        star.AddObjectType(
            AstronomicalObjectType.Create(
                candidate.ObjectType,
                candidate.ObjectType,
                "Imported by the scheduled astronomical research job."));
        if (candidate.RightAscensionDegrees.HasValue &&
            candidate.DeclinationDegrees.HasValue)
        {
            star.AddCoordinate(
                AstronomicalCoordinate.Create(
                    candidate.RightAscensionDegrees.Value,
                    candidate.DeclinationDegrees.Value,
                    CoordinateSystem.ICRS,
                    2000.0));
        }

        if (candidate.ParallaxMas.HasValue)
            star.AddParallax(
                ParallaxMeasurement.Create(candidate.ParallaxMas.Value));

        return star;
    }

    private static string BuildSurveyImageUrl(
        string survey,
        double rightAscensionDegrees,
        double declinationDegrees)
    {
        var encodedSurvey = Uri.EscapeDataString(survey);
        return FormattableString.Invariant(
            $"https://alasky.cds.unistra.fr/hips-image-services/hips2fits?hips={encodedSurvey}&ra={rightAscensionDegrees:R}&dec={declinationDegrees:R}&fov=0.12&width=800&height=600&format=jpg");
    }

    private sealed record ResearchCheckpoint(string? AfterMainId);
}
