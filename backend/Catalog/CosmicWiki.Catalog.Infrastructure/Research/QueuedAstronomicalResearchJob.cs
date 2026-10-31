using CosmicWiki.Catalog.Application.Research;
using Hangfire;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class QueuedAstronomicalResearchJob(
    IAstronomicalResearchJob researchJob,
    IAstronomicalResearchProvider provider,
    IBackgroundJobClient backgroundJobClient)
{
    [Queue("research")]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var source = await provider.GetStatusAsync(cancellationToken);
        if (!source.IsReachable)
        {
            backgroundJobClient.Schedule<QueuedAstronomicalResearchJob>(
                job => job.RunAsync(CancellationToken.None),
                TimeSpan.FromMinutes(5));
            throw new HttpRequestException(source.Message ?? "SIMBAD is unavailable.");
        }

        await researchJob.RunAsync(cancellationToken);
    }
}
