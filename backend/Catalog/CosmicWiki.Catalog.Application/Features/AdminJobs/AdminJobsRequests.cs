using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AdminJobs;

public sealed record GetAstronomicalResearchStatusQuery
    : IRequest<AstronomicalResearchJobStatus>;

public sealed record EnqueueAstronomicalResearchCommand
    : IRequest<string>;

public sealed record GetAstronomicalResearchRunQuery(string JobId)
    : IRequest<AstronomicalResearchRunStatus?>;

public sealed record GetHangfireOverviewQuery
    : IRequest<HangfireOverviewResponse>;

public sealed record GetHangfireJobDetailsQuery(string JobId)
    : IRequest<HangfireJobDetailsResponse?>;

public sealed record GetHangfireJobsQuery(
    string State,
    int Page,
    int PageSize,
    int? From,
    int? Count,
    string? Queue) : IRequest<HangfireJobsPage?>;

public sealed record GetHangfireRetriesQuery(int Page, int PageSize)
    : IRequest<HangfireRetriesPage>;

public sealed record GetHangfireRecurringJobsQuery(int Page, int PageSize)
    : IRequest<HangfireRecurringJobsPage>;

public sealed record TriggerHangfireRecurringJobCommand(string JobId)
    : IRequest<Unit>;

public sealed record DeleteHangfireRecurringJobCommand(string JobId)
    : IRequest<bool>;

public sealed record RequeueHangfireJobCommand(string JobId)
    : IRequest<bool>;

public sealed record DeleteHangfireJobCommand(string JobId)
    : IRequest<bool>;
