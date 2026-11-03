using CosmicWiki.Caching;
using CosmicWiki.Catalog.Application.Research;
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
    private readonly IAstronomicalResearchCandidateProcessor _candidateProcessor;
    private readonly AstronomicalResearchOptions _options;
    private readonly ILogger<AstronomicalResearchJob> _logger;
    private readonly ICacheStore _cacheStore;

    public AstronomicalResearchJob(
        IAstronomicalResearchProvider provider,
        IAstronomicalResearchCandidateProcessor candidateProcessor,
        IOptions<AstronomicalResearchOptions> options,
        ILogger<AstronomicalResearchJob> logger,
        ICacheStore cacheStore)
    {
        _provider = provider;
        _candidateProcessor = candidateProcessor;
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

        if (!await RunLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogInformation(
                "Astronomical research job skipped because another run is already in progress.");
            return;
        }

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
            var pageResult = await _candidateProcessor.ProcessPageAsync(
                uniqueCandidates,
                cancellationToken);
            createdCount += pageResult.CreatedCount;
            existingCount += pageResult.ExistingCount;

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
