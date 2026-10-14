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
        [FromQuery] int from = 0,
        [FromQuery] int count = 25)
    {
        count = Math.Clamp(count, 1, 100);
        from = Math.Max(0, from);
        var monitoring = JobStorage.Current.GetMonitoringApi();

        var normalizedState = state.ToLowerInvariant();
        var jobs = normalizedState switch
        {
            "enqueued" => monitoring.EnqueuedJobs("default", from, count)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "scheduled" => monitoring.ScheduledJobs(from, count)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "processing" => monitoring.ProcessingJobs(from, count)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "succeeded" => monitoring.SucceededJobs(from, count)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "failed" => monitoring.FailedJobs(from, count)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            "deleted" => monitoring.DeletedJobs(from, count)
                .Select(item => ToHangfireJobSummary(item.Key, item.Value.Job.Method.Name)),
            _ => throw new ArgumentException($"Unknown Hangfire state '{state}'.")
        };

        return Ok(new { state = normalizedState, jobs });
    }

    [HttpGet("hangfire/recurring")]
    public IActionResult GetRecurringJobs()
    {
        var jobs = JobStorage.Current
            .GetConnection()
            .GetRecurringJobs()
            .Select(job => new
            {
                id = job.Id,
                cron = job.Cron,
                queue = job.Queue,
                nextExecution = job.NextExecution,
                lastExecution = job.LastExecution,
                lastJobId = job.LastJobId,
                error = job.Error
            });

        return Ok(jobs);
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
