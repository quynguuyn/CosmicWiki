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
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly AstronomicalResearchOptions _options;
    private readonly ILogger<AstronomicalResearchJob> _logger;
    private readonly ICacheStore _cacheStore;

    public AstronomicalResearchJob(
        IAstronomicalResearchProvider provider,
        IStarRepository starRepository,
        IAstronomicalObjectRepository objectRepository,
        ICatalogUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IOptions<AstronomicalResearchOptions> options,
        ILogger<AstronomicalResearchJob> logger,
        ICacheStore cacheStore)
    {
        _provider = provider;
        _starRepository = starRepository;
        _objectRepository = objectRepository;
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
            var pageCreatedCount = 0;

            foreach (var candidate in uniqueCandidates)
            {
                if (existingSet.Contains(candidate.CatalogValue))
                {
                    existingCount++;
                    continue;
                }

                var astronomicalObject = AstronomicalObject.Create(
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

                if (candidate.Kind == AstronomicalObjectKind.Star)
                {
                    var star = Star.Create(astronomicalObject.Id, candidate.Name);
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

                    await _starRepository.AddAsync(star, cancellationToken);

                    _outboxWriter.Add(
                        new StarCreatedIntegrationEvent(
                            Guid.NewGuid(),
                            1,
                            DateTime.UtcNow,
                            star.Id,
                            star.Id,
                            candidate.Name,
                            null,
                            null),
                        KafkaTopics.CatalogStarCreated);
                }
                else
                {
                    _outboxWriter.Add(
                        new AstronomicalObjectCreatedIntegrationEvent(
                            Guid.NewGuid(),
                            1,
                            DateTime.UtcNow,
                            astronomicalObject.Id,
                            astronomicalObject.Id,
                            candidate.Kind.ToString(),
                            candidate.Name,
                            candidate.ObjectType),
                        KafkaTopics.CatalogAstronomicalObjectCreated);
                }

                createdCount++;
                pageCreatedCount++;
            }

            if (pageCreatedCount > 0)
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

    private sealed record ResearchCheckpoint(string? AfterMainId);
}
