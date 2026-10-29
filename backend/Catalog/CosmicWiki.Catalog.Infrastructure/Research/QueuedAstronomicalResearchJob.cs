using CosmicWiki.Catalog.Application.Research;
using Hangfire;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class QueuedAstronomicalResearchJob(
    IAstronomicalResearchJob researchJob)
{
    [Queue("research")]
    public Task RunAsync(CancellationToken cancellationToken) =>
        researchJob.RunAsync(cancellationToken);
}
