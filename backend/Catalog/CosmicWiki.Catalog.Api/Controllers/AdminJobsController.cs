using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Caching;
using CosmicWiki.SharedKernel.Authorization;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CosmicWiki.Catalog.Api.Controllers;

[ApiController]
[Route("api/admin/jobs")]
[Authorize(Policy = Permissions.CatalogWrite)]
public sealed class AdminJobsController(
    IBackgroundJobClient backgroundJobClient,
    IOptions<AstronomicalResearchOptions> researchOptions,
    ICacheStore cacheStore,
    ILogger<AdminJobsController> logger) : ControllerBase
{
    [HttpGet("astronomical-research")]
    public IActionResult GetAstronomicalResearchStatus()
    {
        var options = researchOptions.Value;

        return Ok(new
        {
            id = "astronomical-research",
            name = "Nghiên cứu thiên văn",
            enabled = options.Enabled,
            schedule = "02:00 UTC mỗi ngày",
            pageSize = options.PageSize,
            maxPagesPerRun = options.MaxPagesPerRun,
            maxRecordsPerRun = Math.Clamp(options.PageSize, 1, 500) *
                Math.Clamp(options.MaxPagesPerRun, 1, 100),
            source = "SIMBAD"
        });
    }

    [HttpPost("astronomical-research/run")]
    public IActionResult RunAstronomicalResearch()
    {
        var jobId = backgroundJobClient.Enqueue<IAstronomicalResearchJob>(
            job => job.RunAsync(CancellationToken.None));

        return Accepted(new
        {
            jobId,
            message = "Job nghiên cứu thiên văn đã được đưa vào hàng đợi."
        });
    }

    [HttpGet("astronomical-research/runs/{jobId}")]
    public async Task<IActionResult> GetAstronomicalResearchRun(
        string jobId,
        CancellationToken cancellationToken)
    {
        var details = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
        if (details is null)
            return NotFound();

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

        return Ok(new
        {
            jobId,
            state = details.History.FirstOrDefault()?.StateName ?? "Unknown",
            lastRun
        });
    }

    [HttpGet("hangfire")]
    public IActionResult GetHangfireOverview()
    {
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var statistics = monitoring.GetStatistics();

        return Ok(new
        {
            jobs = new
            {
                enqueued = statistics.Enqueued,
                scheduled = statistics.Scheduled,
                processing = statistics.Processing,
                succeeded = statistics.Succeeded,
                failed = statistics.Failed,
                deleted = statistics.Deleted,
                awaiting = statistics.Awaiting,
                retries = statistics.Retries
            },
            servers = monitoring.Servers().Select(server => new
            {
                name = server.Name,
                workers = server.WorkersCount,
                queues = server.Queues,
                startedAtUtc = server.StartedAt,
                heartbeatUtc = server.Heartbeat
            })
        });
    }

    [HttpGet("hangfire/jobs/{state}")]
    public IActionResult GetHangfireJobs(
        string state,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] int? from = null,
        [FromQuery] int? count = null)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var pageOffset = from.HasValue
            ? Math.Max(0, from.Value)
            : (page - 1) * pageSize;
        var take = Math.Clamp(count ?? pageSize, 1, 100);
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var statistics = monitoring.GetStatistics();

        var normalizedState = state.ToLowerInvariant();
        var totalCount = normalizedState switch
        {
            "enqueued" => statistics.Enqueued,
            "scheduled" => statistics.Scheduled,
            "processing" => statistics.Processing,
            "succeeded" => statistics.Succeeded,
            "failed" => statistics.Failed,
            "deleted" => statistics.Deleted,
            _ => -1
        };
        if (totalCount < 0)
            return BadRequest($"Unknown Hangfire state '{state}'.");

        if (!from.HasValue)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = totalPages == 0 ? 1 : Math.Min(page, totalPages);
            pageOffset = (page - 1) * pageSize;
        }

        IEnumerable<HangfireJobSummary> jobs = normalizedState switch
        {
            "enqueued" => monitoring.EnqueuedJobs("default", pageOffset, take)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "scheduled" => monitoring.ScheduledJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "processing" => monitoring.ProcessingJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "succeeded" => monitoring.SucceededJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "failed" => monitoring.FailedJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "deleted" => monitoring.DeletedJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            _ => Array.Empty<HangfireJobSummary>()
        };

        var effectivePageSize = count.HasValue ? take : pageSize;
        var effectivePage = from.HasValue
            ? pageOffset / effectivePageSize + 1
            : page;

        return Ok(new
        {
            state = normalizedState,
            jobs,
            page = effectivePage,
            pageSize = effectivePageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)effectivePageSize),
            hasPreviousPage = effectivePage > 1,
            hasNextPage = effectivePage < (int)Math.Ceiling(totalCount / (double)effectivePageSize)
        });
    }

    [HttpGet("hangfire/recurring")]
    public IActionResult GetRecurringJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var allJobs = JobStorage.Current
            .GetConnection()
            .GetRecurringJobs()
            .OrderBy(job => job.Id, StringComparer.Ordinal)
            .Select(job => new
            {
                id = job.Id,
                cron = job.Cron,
                queue = job.Queue,
                nextExecution = job.NextExecution,
                lastExecution = job.LastExecution,
                lastJobId = job.LastJobId,
                error = job.Error
            })
            .ToArray();
        var totalCount = allJobs.Length;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);
        var jobs = allJobs.Skip((page - 1) * pageSize).Take(pageSize);

        return Ok(new
        {
            jobs,
            page,
            pageSize,
            totalCount,
            totalPages,
            hasPreviousPage = page > 1,
            hasNextPage = page < totalPages
        });
    }

    [HttpPost("hangfire/recurring/{jobId}/trigger")]
    public IActionResult TriggerRecurringJob(string jobId)
    {
        new RecurringJobManager().Trigger(jobId);
        return Accepted(new { jobId, message = "Recurring job đã được kích hoạt." });
    }

    [HttpPost("hangfire/jobs/{jobId}/requeue")]
    public IActionResult RequeueJob(string jobId)
    {
        return backgroundJobClient.Requeue(jobId)
            ? Accepted(new { jobId, message = "Job đã được đưa lại vào hàng đợi." })
            : NotFound();
    }

    [HttpDelete("hangfire/jobs/{jobId}")]
    public IActionResult DeleteJob(string jobId)
    {
        return backgroundJobClient.Delete(jobId)
            ? NoContent()
            : NotFound();
    }

    private static HangfireJobSummary ToHangfireJobSummary(string id, string methodName) =>
        new(id, new(new(methodName)));

    private sealed record HangfireJobSummary(string Id, HangfireJobDetails Job);

    private sealed record HangfireJobDetails(HangfireJobMethod Method);

    private sealed record HangfireJobMethod(string Name);
}
