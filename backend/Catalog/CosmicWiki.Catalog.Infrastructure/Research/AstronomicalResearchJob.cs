using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Domain.Entities;
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

        var startedAtUtc = DateTime.UtcNow;
        var candidates = await _provider.FindCandidatesAsync(cancellationToken);
        var createdCount = 0;
        var existingCount = 0;
        var seenIdentifiers = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in candidates)
        {
            var identifierKey =
                $"{candidate.Catalog}:{candidate.CatalogValue}";

            if (!seenIdentifiers.Add(identifierKey))
                continue;

            var existing = await _starRepository.GetByCatalogIdentifierAsync(
                candidate.Catalog,
                candidate.CatalogValue,
                cancellationToken);

            if (existing is not null)
            {
                existingCount++;
                continue;
            }

            var astronomicalObject = AstronomicalObject.Create();
            var star = Star.Create(astronomicalObject.Id, candidate.Name);
            star.AddCatalogIdentifier(
                CatalogIdentifier.Create(candidate.Catalog, candidate.CatalogValue));
            star.AddObjectType(
                AstronomicalObjectType.Create(
                    candidate.ObjectType,
                    candidate.ObjectType,
                    "Imported by the scheduled astronomical research job."));

            await _objectRepository.AddAsync(astronomicalObject, cancellationToken);
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

            createdCount++;
        }

        if (createdCount > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheStore.SetAsync(
            "admin:jobs:astronomical-research:last-run",
            new AstronomicalResearchRunResult(
                startedAtUtc,
                DateTime.UtcNow,
                candidates.Count,
                existingCount,
                createdCount),
            TimeSpan.FromDays(7),
            cancellationToken);

        _logger.LogInformation(
            "Astronomical research job completed. Candidates: {CandidateCount}, existing: {ExistingCount}, created: {CreatedCount}.",
            candidates.Count,
            existingCount,
            createdCount);
    }
}
