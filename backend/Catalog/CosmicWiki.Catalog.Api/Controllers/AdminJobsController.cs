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
                awaiting = statistics.Awaiting ?? 0,
                retries = statistics.Retries ?? 0,
                recurring = statistics.Recurring,
                servers = statistics.Servers,
                queues = statistics.Queues
            },
            queues = monitoring.Queues().Select(queue => new
            {
                name = queue.Name,
                length = queue.Length,
                fetched = queue.Fetched,
                nextJobs = queue.FirstJobs.Select(item =>
                    ToHangfireJobSummary(item.Key, "enqueued", item.Value.Job, queue.Name))
            }),
            hourlySucceeded = monitoring.HourlySucceededJobs().OrderBy(item => item.Key)
                .Select(item => new { atUtc = item.Key, count = item.Value }),
            hourlyFailed = monitoring.HourlyFailedJobs().OrderBy(item => item.Key)
                .Select(item => new { atUtc = item.Key, count = item.Value }),
            dailySucceeded = monitoring.SucceededByDatesCount().OrderBy(item => item.Key)
                .Select(item => new { atUtc = item.Key, count = item.Value }),
            dailyFailed = monitoring.FailedByDatesCount().OrderBy(item => item.Key)
                .Select(item => new { atUtc = item.Key, count = item.Value }),
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

    [HttpGet("hangfire/job-details/{jobId}")]
    public IActionResult GetHangfireJobDetails(string jobId)
    {
        var details = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
        if (details is null)
            return NotFound();

        var job = details.Job;
        var parameters = job.Method.GetParameters();
        var arguments = job.Args
            .Select((argument, index) => new
            {
                name = index < parameters.Length
                    ? parameters[index].Name
                    : $"argument{index + 1}",
                value = argument?.ToString() ?? "null"
            });

        var currentState = details.History.FirstOrDefault();
        var lastError = details.History.FirstOrDefault(history =>
            history.Data.ContainsKey("ExceptionMessage"));

        return Ok(new
        {
            id = jobId,
            type = job.Type.FullName ?? job.Type.Name,
            method = job.Method.Name,
            currentState = currentState?.StateName ?? "Unknown",
            createdAt = details.CreatedAt,
            expireAt = details.ExpireAt,
            parameters = details.Properties,
            arguments,
            retryCount = details.Properties.TryGetValue("RetryCount", out var retryCount)
                ? retryCount
                : null,
            exceptionType = lastError is null ? null : GetStateData(lastError.Data, "ExceptionType"),
            exceptionMessage = lastError is null ? null : GetStateData(lastError.Data, "ExceptionMessage"),
            exceptionDetails = lastError is null ? null : GetStateData(lastError.Data, "ExceptionDetails"),
            history = details.History.Select(state => new
            {
                state = state.StateName,
                reason = state.Reason,
                at = state.CreatedAt,
                data = state.Data
            })
        });
    }

    [HttpGet("hangfire/jobs/{state}")]
    public IActionResult GetHangfireJobs(
        string state,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] int? from = null,
        [FromQuery] int? count = null,
        [FromQuery] string? queue = null)
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
        var selectedQueue = string.IsNullOrWhiteSpace(queue) ? "default" : queue.Trim();
        var totalCount = normalizedState switch
        {
            "enqueued" => (int)monitoring.EnqueuedCount(selectedQueue),
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

        using var storageConnection = JobStorage.Current.GetConnection();
        IEnumerable<HangfireJobSummary> jobs = normalizedState switch
        {
            "enqueued" => monitoring.EnqueuedJobs(selectedQueue, pageOffset, take)
                .Select(item => ToHangfireJobSummary(
                    item.Key, normalizedState, item.Value.Job, selectedQueue, item.Value.EnqueuedAt)),
            "scheduled" => monitoring.ScheduledJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(
                    item.Key, normalizedState, item.Value.Job, timestamp: item.Value.EnqueueAt,
                    retryCount: storageConnection.GetJobParameter(item.Key, "RetryCount"),
                    error: GetStateData(item.Value.StateData, "ExceptionMessage"))),
            "processing" => monitoring.ProcessingJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(
                    item.Key, normalizedState, item.Value.Job, item.Value.ServerId, item.Value.StartedAt)),
            "succeeded" => monitoring.SucceededJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(
                    item.Key, normalizedState, item.Value.Job, timestamp: item.Value.SucceededAt,
                    duration: item.Value.TotalDuration)),
            "failed" => monitoring.FailedJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(
                    item.Key, normalizedState, item.Value.Job, timestamp: item.Value.FailedAt,
                    error: item.Value.ExceptionMessage)),
            "deleted" => monitoring.DeletedJobs(pageOffset, take)
                .Select(item => ToHangfireJobSummary(item.Key, normalizedState, item.Value.Job)),
            _ => Array.Empty<HangfireJobSummary>()
        };

        var effectivePageSize = count.HasValue ? take : pageSize;
        var effectivePage = from.HasValue
            ? pageOffset / effectivePageSize + 1
            : page;

        return Ok(new
        {
            state = normalizedState,
            queue = normalizedState == "enqueued" ? selectedQueue : null,
            jobs,
            page = effectivePage,
            pageSize = effectivePageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)effectivePageSize),
            hasPreviousPage = effectivePage > 1,
            hasNextPage = effectivePage < (int)Math.Ceiling(totalCount / (double)effectivePageSize)
        });
    }

    [HttpGet("hangfire/retries")]
    public IActionResult GetHangfireRetries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var monitoring = JobStorage.Current.GetMonitoringApi();
        var totalScheduledCount = monitoring.ScheduledCount();
        const int scanLimit = 5000;
        var scheduledCount = (int)Math.Min(totalScheduledCount, scanLimit);
        var retries = new List<HangfireJobSummary>();

        using (var connection = JobStorage.Current.GetConnection())
        {
            for (var offset = 0; offset < scheduledCount; offset += 100)
            {
                var batch = monitoring.ScheduledJobs(offset, Math.Min(100, scheduledCount - offset));
                foreach (var item in batch)
                {
                    var retryCount = connection.GetJobParameter(item.Key, "RetryCount");
                    if (int.TryParse(retryCount, out var attempt) && attempt > 0)
                    {
                        retries.Add(ToHangfireJobSummary(
                            item.Key,
                            "scheduled",
                            item.Value.Job,
                            timestamp: item.Value.EnqueueAt,
                            retryCount: retryCount,
                            error: GetStateData(item.Value.StateData, "ExceptionMessage")));
                    }
                }
            }
        }

        var totalCount = retries.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);
        var jobs = retries.Skip((page - 1) * pageSize).Take(pageSize);

        return Ok(new
        {
            jobs,
            page,
            pageSize,
            totalCount,
            totalPages,
            hasPreviousPage = page > 1,
            hasNextPage = page < totalPages,
            scanLimit,
            totalScheduledCount,
            scannedScheduledCount = scheduledCount,
            isScanLimited = totalScheduledCount > scheduledCount
        });
    }

    [HttpGet("hangfire/recurring")]
    public IActionResult GetRecurringJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        using var connection = JobStorage.Current.GetConnection();
        var allJobs = connection
            .GetRecurringJobs()
            .OrderBy(job => job.Id, StringComparer.Ordinal)
            .Select(job => new
            {
                id = job.Id,
                cron = job.Cron,
                queue = job.Queue,
                timeZoneId = job.TimeZoneId,
                method = job.Job is null ? null : job.Job.Method.Name,
                type = job.Job?.Type.FullName,
                nextExecution = job.NextExecution,
                lastExecution = job.LastExecution,
                lastJobId = job.LastJobId,
                createdAt = job.CreatedAt,
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

    [HttpDelete("hangfire/recurring/{jobId}")]
    public IActionResult DeleteRecurringJob(string jobId)
    {
        using var connection = JobStorage.Current.GetConnection();
        var jobs = connection.GetRecurringJobs();
        if (jobs.All(job => !string.Equals(job.Id, jobId, StringComparison.Ordinal)))
            return NotFound();

        new RecurringJobManager().RemoveIfExists(jobId);
        return NoContent();
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

    private static HangfireJobSummary ToHangfireJobSummary(
        string id,
        string state,
        Hangfire.Common.Job job,
        string? queue = null,
        DateTime? timestamp = null,
        string? retryCount = null,
        string? error = null,
        long? duration = null) =>
        new(
            id,
            state,
            job.Type.FullName ?? job.Type.Name,
            job.Method.Name,
            queue,
            timestamp,
            retryCount,
            error,
            duration);

    private static string? GetStateData(IDictionary<string, string>? data, string key) =>
        data is not null && data.TryGetValue(key, out var value) ? value : null;

    private sealed record HangfireJobSummary(
        string Id,
        string State,
        string Type,
        string Method,
        string? Queue,
        DateTime? Timestamp,
        string? RetryCount,
        string? Error,
        long? Duration);
}
