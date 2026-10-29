using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using CosmicWiki.Catalog.Application.Features.AdminJobs;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class HangfireDashboardOperations(
    IBackgroundJobClient backgroundJobClient,
    IRecurringJobManager recurringJobManager,
    JobStorage jobStorage) : IHangfireDashboardOperations
{
    private const int RetryScanLimit = 5000;

    public HangfireOverviewResponse GetHangfireOverview()
    {
        var monitoring = jobStorage.GetMonitoringApi();
        var statistics = monitoring.GetStatistics();
        var queues = monitoring.Queues()
            .Select(queue => new HangfireQueueResponse(
                queue.Name,
                queue.Length,
                queue.Fetched,
                queue.FirstJobs.Select(item =>
                    ToHangfireJobSummary(item.Key, "enqueued", item.Value.Job, queue.Name))
                    .ToArray()))
            .ToArray();

        return new HangfireOverviewResponse(
            new HangfireStatisticsResponse(
                statistics.Enqueued,
                statistics.Scheduled,
                statistics.Processing,
                statistics.Succeeded,
                statistics.Failed,
                statistics.Deleted,
                statistics.Awaiting ?? 0,
                statistics.Retries ?? 0,
                statistics.Recurring,
                statistics.Servers,
                statistics.Queues),
            queues,
            ToTimeCounts(monitoring.HourlySucceededJobs()),
            ToTimeCounts(monitoring.HourlyFailedJobs()),
            ToTimeCounts(monitoring.SucceededByDatesCount()),
            ToTimeCounts(monitoring.FailedByDatesCount()),
            monitoring.Servers().Select(server => new HangfireServerResponse(
                server.Name,
                server.WorkersCount,
                server.Queues.ToArray(),
                server.StartedAt,
                server.Heartbeat))
                .ToArray());
    }

    public HangfireJobDetailsResponse? GetHangfireJobDetails(string jobId)
    {
        var details = jobStorage.GetMonitoringApi().JobDetails(jobId);
        if (details is null)
            return null;

        var job = details.Job;
        var parameters = job.Method.GetParameters();
        var arguments = job.Args
            .Select((argument, index) => new HangfireJobArgument(
                index < parameters.Length ? parameters[index].Name ?? $"argument{index + 1}" : $"argument{index + 1}",
                argument?.ToString() ?? "null"))
            .ToArray();
        var currentState = details.History.FirstOrDefault();
        var lastError = details.History.FirstOrDefault(history =>
            history.Data.ContainsKey("ExceptionMessage"));

        return new HangfireJobDetailsResponse(
            jobId,
            job.Type.FullName ?? job.Type.Name,
            job.Method.Name,
            currentState?.StateName ?? "Unknown",
            details.CreatedAt,
            details.ExpireAt,
            details.Properties,
            arguments,
            details.Properties.TryGetValue("RetryCount", out var retryCount)
                ? retryCount
                : null,
            GetStateData(lastError?.Data, "ExceptionType"),
            GetStateData(lastError?.Data, "ExceptionMessage"),
            GetStateData(lastError?.Data, "ExceptionDetails"),
            details.History.Select(state => new HangfireHistoryEntry(
                state.StateName,
                state.Reason,
                state.CreatedAt,
                state.Data))
                .ToArray());
    }

    public HangfireJobsPage? GetHangfireJobs(
        string state,
        int page,
        int pageSize,
        int? from,
        int? count,
        string? queue)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var pageOffset = from.HasValue
            ? Math.Max(0, from.Value)
            : (page - 1) * pageSize;
        var take = Math.Clamp(count ?? pageSize, 1, 100);
        var monitoring = jobStorage.GetMonitoringApi();
        var statistics = monitoring.GetStatistics();
        var normalizedState = state.ToLowerInvariant();
        var selectedQueue = string.IsNullOrWhiteSpace(queue) ? "default" : queue.Trim();
        var totalCount = normalizedState switch
        {
            "enqueued" => monitoring.EnqueuedCount(selectedQueue),
            "scheduled" => statistics.Scheduled,
            "processing" => statistics.Processing,
            "succeeded" => statistics.Succeeded,
            "failed" => statistics.Failed,
            "deleted" => statistics.Deleted,
            _ => -1L
        };
        if (totalCount < 0)
            return null;

        if (!from.HasValue)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = totalPages == 0 ? 1 : Math.Min(page, totalPages);
            pageOffset = (page - 1) * pageSize;
        }

        using var storageConnection = jobStorage.GetConnection();
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
        var totalPagesForPageSize = (int)Math.Ceiling(totalCount / (double)effectivePageSize);

        return new HangfireJobsPage(
            normalizedState,
            normalizedState == "enqueued" ? selectedQueue : null,
            jobs.ToArray(),
            effectivePage,
            effectivePageSize,
            totalCount,
            totalPagesForPageSize,
            effectivePage > 1,
            effectivePage < totalPagesForPageSize);
    }

    public HangfireRetriesPage GetHangfireRetries(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var monitoring = jobStorage.GetMonitoringApi();
        var totalScheduledCount = monitoring.ScheduledCount();
        var scheduledCount = (int)Math.Min(totalScheduledCount, RetryScanLimit);
        var retries = new List<HangfireJobSummary>();

        using (var connection = jobStorage.GetConnection())
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

        return new HangfireRetriesPage(
            retries.Skip((page - 1) * pageSize).Take(pageSize).ToArray(),
            page,
            pageSize,
            totalCount,
            totalPages,
            page > 1,
            page < totalPages,
            RetryScanLimit,
            totalScheduledCount,
            scheduledCount,
            totalScheduledCount > scheduledCount);
    }

    public HangfireRecurringJobsPage GetRecurringJobs(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        using var connection = jobStorage.GetConnection();
        var allJobs = connection.GetRecurringJobs()
            .OrderBy(job => job.Id, StringComparer.Ordinal)
            .Select(job => new HangfireRecurringJobResponse(
                job.Id,
                job.Cron,
                job.Queue,
                job.TimeZoneId,
                job.Job?.Method.Name,
                job.Job?.Type.FullName,
                job.NextExecution,
                job.LastExecution,
                job.LastJobId,
                job.CreatedAt,
                job.Error))
            .ToArray();
        var totalCount = allJobs.Length;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);

        return new HangfireRecurringJobsPage(
            allJobs.Skip((page - 1) * pageSize).Take(pageSize).ToArray(),
            page,
            pageSize,
            totalCount,
            totalPages,
            page > 1,
            page < totalPages);
    }

    public void TriggerRecurringJob(string jobId) =>
        recurringJobManager.Trigger(jobId);

    public bool DeleteRecurringJob(string jobId)
    {
        using var connection = jobStorage.GetConnection();
        if (connection.GetRecurringJobs().All(job =>
            !string.Equals(job.Id, jobId, StringComparison.Ordinal)))
            return false;

        recurringJobManager.RemoveIfExists(jobId);
        return true;
    }

    public bool RequeueJob(string jobId) =>
        backgroundJobClient.Requeue(jobId);

    public bool DeleteJob(string jobId) =>
        backgroundJobClient.Delete(jobId);

    private static IReadOnlyCollection<HangfireCountByTime> ToTimeCounts(
        IDictionary<DateTime, long> values) =>
        values.OrderBy(item => item.Key)
            .Select(item => new HangfireCountByTime(item.Key, item.Value))
            .ToArray();

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
}
