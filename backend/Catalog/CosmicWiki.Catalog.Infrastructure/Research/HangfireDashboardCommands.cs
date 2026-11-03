using CosmicWiki.Catalog.Application.Features.AdminJobs;
using Hangfire;
using Hangfire.Storage;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class HangfireDashboardCommands(
    IBackgroundJobClient backgroundJobClient,
    IRecurringJobManager recurringJobManager,
    JobStorage jobStorage) : IHangfireDashboardCommands
{
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
}
