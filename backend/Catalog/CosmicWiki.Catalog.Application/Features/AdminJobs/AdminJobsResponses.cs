using CosmicWiki.Catalog.Application.Research;

namespace CosmicWiki.Catalog.Application.Features.AdminJobs;

public sealed record AstronomicalResearchJobStatus(
    string Id,
    string Name,
    bool Enabled,
    string Schedule,
    int PageSize,
    int MaxPagesPerRun,
    int MaxRecordsPerRun,
    string Source,
    bool SourceReachable,
    string? SourceStatus,
    DateTime SourceCheckedAtUtc);

public sealed record AstronomicalResearchRunStatus(
    string JobId,
    string State,
    AstronomicalResearchRunResult? LastRun);

public sealed record HangfireOverviewResponse(
    HangfireStatisticsResponse Jobs,
    IReadOnlyCollection<HangfireQueueResponse> Queues,
    IReadOnlyCollection<HangfireCountByTime> HourlySucceeded,
    IReadOnlyCollection<HangfireCountByTime> HourlyFailed,
    IReadOnlyCollection<HangfireCountByTime> DailySucceeded,
    IReadOnlyCollection<HangfireCountByTime> DailyFailed,
    IReadOnlyCollection<HangfireServerResponse> Servers);

public sealed record HangfireStatisticsResponse(
    long Enqueued,
    long Scheduled,
    long Processing,
    long Succeeded,
    long Failed,
    long Deleted,
    long Awaiting,
    long Retries,
    long Recurring,
    long Servers,
    long Queues);

public sealed record HangfireQueueResponse(
    string Name,
    long Length,
    long? Fetched,
    IReadOnlyCollection<HangfireJobSummary> NextJobs);

public sealed record HangfireCountByTime(DateTime AtUtc, long Count);

public sealed record HangfireServerResponse(
    string Name,
    int Workers,
    IReadOnlyCollection<string> Queues,
    DateTime StartedAtUtc,
    DateTime? HeartbeatUtc);

public sealed record HangfireJobDetailsResponse(
    string Id,
    string Type,
    string Method,
    string CurrentState,
    DateTime? CreatedAt,
    DateTime? ExpireAt,
    IDictionary<string, string> Parameters,
    IReadOnlyCollection<HangfireJobArgument> Arguments,
    string? RetryCount,
    string? ExceptionType,
    string? ExceptionMessage,
    string? ExceptionDetails,
    IReadOnlyCollection<HangfireHistoryEntry> History);

public sealed record HangfireJobArgument(string Name, string Value);

public sealed record HangfireHistoryEntry(
    string State,
    string? Reason,
    DateTime? At,
    IDictionary<string, string> Data);

public sealed record HangfireJobSummary(
    string Id,
    string State,
    string Type,
    string Method,
    string? Queue,
    DateTime? Timestamp,
    string? RetryCount,
    string? Error,
    long? Duration);

public sealed record HangfireJobsPage(
    string State,
    string? Queue,
    IReadOnlyCollection<HangfireJobSummary> Jobs,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);

public sealed record HangfireRetriesPage(
    IReadOnlyCollection<HangfireJobSummary> Jobs,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage,
    int ScanLimit,
    long TotalScheduledCount,
    int ScannedScheduledCount,
    bool IsScanLimited);

public sealed record HangfireRecurringJobResponse(
    string Id,
    string Cron,
    string Queue,
    string TimeZoneId,
    string? Method,
    string? Type,
    DateTime? NextExecution,
    DateTime? LastExecution,
    string? LastJobId,
    DateTime? CreatedAt,
    string? Error);

public sealed record HangfireRecurringJobsPage(
    IReadOnlyCollection<HangfireRecurringJobResponse> Jobs,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage);
