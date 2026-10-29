using CosmicWiki.Caching;
using CosmicWiki.Catalog.Application.Features.AdminJobs;
using CosmicWiki.Catalog.Application.Research;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class AstronomicalResearchAdminOperations(
    IBackgroundJobClient backgroundJobClient,
    JobStorage jobStorage,
    IOptions<AstronomicalResearchOptions> researchOptions,
    ICacheStore cacheStore,
    ILogger<AstronomicalResearchAdminOperations> logger) : IAstronomicalResearchAdminOperations
{
    public AstronomicalResearchJobStatus GetAstronomicalResearchStatus()
    {
        var options = researchOptions.Value;
        return new AstronomicalResearchJobStatus(
            "astronomical-research",
            "Nghiên cứu thiên văn",
            options.Enabled,
            "02:00 UTC mỗi ngày",
            options.PageSize,
            options.MaxPagesPerRun,
            Math.Clamp(options.PageSize, 1, 500) *
                Math.Clamp(options.MaxPagesPerRun, 1, 100),
            "SIMBAD");
    }

    public string EnqueueAstronomicalResearch() =>
        backgroundJobClient.Enqueue<QueuedAstronomicalResearchJob>(
            job => job.RunAsync(CancellationToken.None));

    public async Task<AstronomicalResearchRunStatus?> GetAstronomicalResearchRunAsync(
        string jobId,
        CancellationToken cancellationToken)
    {
        var details = jobStorage.GetMonitoringApi().JobDetails(jobId);
        if (details is null)
            return null;

        AstronomicalResearchRunResult? lastRun = null;
        try
        {
            lastRun = await cacheStore.GetAsync<AstronomicalResearchRunResult>(
                "admin:jobs:astronomical-research:last-run",
                cancellationToken);
        }
        catch (RedisConnectionException exception)
        {
            logger.LogWarning(
                exception,
                "Research job status was read without the cached result because Redis is unavailable.");
        }

        return new AstronomicalResearchRunStatus(
            jobId,
            details.History.FirstOrDefault()?.StateName ?? "Unknown",
            lastRun);
    }
}
