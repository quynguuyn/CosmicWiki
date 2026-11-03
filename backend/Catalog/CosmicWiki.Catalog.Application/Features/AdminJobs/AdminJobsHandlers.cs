using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AdminJobs;

public sealed class GetAstronomicalResearchStatusHandler(IAstronomicalResearchAdminOperations operations)
    : IRequestHandler<GetAstronomicalResearchStatusQuery, AstronomicalResearchJobStatus>
{
    public Task<AstronomicalResearchJobStatus> Handle(
        GetAstronomicalResearchStatusQuery request,
        CancellationToken cancellationToken) =>
        operations.GetAstronomicalResearchStatusAsync(cancellationToken);
}

public sealed class EnqueueAstronomicalResearchHandler(IAstronomicalResearchAdminOperations operations)
    : IRequestHandler<EnqueueAstronomicalResearchCommand, string>
{
    public Task<string> Handle(
        EnqueueAstronomicalResearchCommand request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.EnqueueAstronomicalResearch());
}

public sealed class GetAstronomicalResearchRunHandler(IAstronomicalResearchAdminOperations operations)
    : IRequestHandler<GetAstronomicalResearchRunQuery, AstronomicalResearchRunStatus?>
{
    public Task<AstronomicalResearchRunStatus?> Handle(
        GetAstronomicalResearchRunQuery request,
        CancellationToken cancellationToken) =>
        operations.GetAstronomicalResearchRunAsync(request.JobId, cancellationToken);
}

public sealed class GetHangfireOverviewHandler(IHangfireDashboardQueries operations)
    : IRequestHandler<GetHangfireOverviewQuery, HangfireOverviewResponse>
{
    public Task<HangfireOverviewResponse> Handle(
        GetHangfireOverviewQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.GetHangfireOverview());
}

public sealed class GetHangfireJobDetailsHandler(IHangfireDashboardQueries operations)
    : IRequestHandler<GetHangfireJobDetailsQuery, HangfireJobDetailsResponse?>
{
    public Task<HangfireJobDetailsResponse?> Handle(
        GetHangfireJobDetailsQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.GetHangfireJobDetails(request.JobId));
}

public sealed class GetHangfireJobsHandler(IHangfireDashboardQueries operations)
    : IRequestHandler<GetHangfireJobsQuery, HangfireJobsPage?>
{
    public Task<HangfireJobsPage?> Handle(
        GetHangfireJobsQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.GetHangfireJobs(
            request.State,
            request.Page,
            request.PageSize,
            request.From,
            request.Count,
            request.Queue));
}

public sealed class GetHangfireRetriesHandler(IHangfireDashboardQueries operations)
    : IRequestHandler<GetHangfireRetriesQuery, HangfireRetriesPage>
{
    public Task<HangfireRetriesPage> Handle(
        GetHangfireRetriesQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.GetHangfireRetries(request.Page, request.PageSize));
}

public sealed class GetHangfireRecurringJobsHandler(IHangfireDashboardQueries operations)
    : IRequestHandler<GetHangfireRecurringJobsQuery, HangfireRecurringJobsPage>
{
    public Task<HangfireRecurringJobsPage> Handle(
        GetHangfireRecurringJobsQuery request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.GetRecurringJobs(request.Page, request.PageSize));
}

public sealed class TriggerHangfireRecurringJobHandler(IHangfireDashboardCommands operations)
    : IRequestHandler<TriggerHangfireRecurringJobCommand, Unit>
{
    public Task<Unit> Handle(
        TriggerHangfireRecurringJobCommand request,
        CancellationToken cancellationToken)
    {
        operations.TriggerRecurringJob(request.JobId);
        return Task.FromResult(Unit.Value);
    }
}

public sealed class DeleteHangfireRecurringJobHandler(IHangfireDashboardCommands operations)
    : IRequestHandler<DeleteHangfireRecurringJobCommand, bool>
{
    public Task<bool> Handle(
        DeleteHangfireRecurringJobCommand request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.DeleteRecurringJob(request.JobId));
}

public sealed class RequeueHangfireJobHandler(IHangfireDashboardCommands operations)
    : IRequestHandler<RequeueHangfireJobCommand, bool>
{
    public Task<bool> Handle(
        RequeueHangfireJobCommand request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.RequeueJob(request.JobId));
}

public sealed class DeleteHangfireJobHandler(IHangfireDashboardCommands operations)
    : IRequestHandler<DeleteHangfireJobCommand, bool>
{
    public Task<bool> Handle(
        DeleteHangfireJobCommand request,
        CancellationToken cancellationToken) =>
        Task.FromResult(operations.DeleteJob(request.JobId));
}
