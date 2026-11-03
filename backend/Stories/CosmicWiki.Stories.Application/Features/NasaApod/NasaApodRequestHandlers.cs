using MediatR;

namespace CosmicWiki.Stories.Application.Features.NasaApod;

public sealed class GetNasaApodStatusHandler(INasaApodProvider provider)
    : IRequestHandler<GetNasaApodStatusQuery, NasaApodSourceStatus>
{
    public Task<NasaApodSourceStatus> Handle(
        GetNasaApodStatusQuery request,
        CancellationToken cancellationToken) =>
        provider.GetStatusAsync(cancellationToken);
}

public sealed class TriggerNasaApodStoryHandler(
    INasaApodProvider provider,
    INasaApodStoryJobScheduler scheduler)
    : IRequestHandler<TriggerNasaApodStoryCommand, TriggerNasaApodStoryResult>
{
    public async Task<TriggerNasaApodStoryResult> Handle(
        TriggerNasaApodStoryCommand request,
        CancellationToken cancellationToken)
    {
        var status = await provider.GetStatusAsync(cancellationToken);
        return new(
            status,
            status.IsReachable ? scheduler.Enqueue() : null);
    }
}
