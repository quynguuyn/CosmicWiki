namespace CosmicWiki.Catalog.Application.Features.AdminJobs;

public interface IAstronomicalResearchAdminOperations
{
    Task<AstronomicalResearchJobStatus> GetAstronomicalResearchStatusAsync(
        CancellationToken cancellationToken);
    string EnqueueAstronomicalResearch();
    Task<AstronomicalResearchRunStatus?> GetAstronomicalResearchRunAsync(
        string jobId,
        CancellationToken cancellationToken);
}

public interface IHangfireDashboardOperations
{
    HangfireOverviewResponse GetHangfireOverview();
    HangfireJobDetailsResponse? GetHangfireJobDetails(string jobId);
    HangfireJobsPage? GetHangfireJobs(
        string state,
        int page,
        int pageSize,
        int? from,
        int? count,
        string? queue);
    HangfireRetriesPage GetHangfireRetries(int page, int pageSize);
    HangfireRecurringJobsPage GetRecurringJobs(int page, int pageSize);
    void TriggerRecurringJob(string jobId);
    bool DeleteRecurringJob(string jobId);
    bool RequeueJob(string jobId);
    bool DeleteJob(string jobId);
}
