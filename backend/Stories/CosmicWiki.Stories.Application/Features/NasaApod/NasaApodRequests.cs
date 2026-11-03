using MediatR;

namespace CosmicWiki.Stories.Application.Features.NasaApod;

public sealed record GetNasaApodStatusQuery : IRequest<NasaApodSourceStatus>;

public sealed record TriggerNasaApodStoryCommand
    : IRequest<TriggerNasaApodStoryResult>;

public sealed record TriggerNasaApodStoryResult(
    NasaApodSourceStatus Status,
    string? JobId);
