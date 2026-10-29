using CosmicWiki.Catalog.Application.Features.AdminJobs;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Catalog.Api.Controllers;

[ApiController]
[Route("api/admin/jobs")]
[Authorize(Policy = Permissions.CatalogWrite)]
public sealed class AdminJobsController(ISender sender) : ControllerBase
{
    [HttpGet("astronomical-research")]
    public async Task<IActionResult> GetAstronomicalResearchStatus(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(
            new GetAstronomicalResearchStatusQuery(),
            cancellationToken));

    [HttpPost("astronomical-research/run")]
    public async Task<IActionResult> RunAstronomicalResearch(
        CancellationToken cancellationToken)
    {
        var jobId = await sender.Send(
            new EnqueueAstronomicalResearchCommand(),
            cancellationToken);
        return Accepted(new
        {
            jobId,
            message = "Job nghiên cứu thiên văn đã được đưa vào hàng đợi."
        });
    }

    [HttpGet("astronomical-research/runs/{jobId}")]
    public async Task<IActionResult> GetAstronomicalResearchRun(
        string jobId,
        CancellationToken cancellationToken)
    {
        var run = await sender.Send(
            new GetAstronomicalResearchRunQuery(jobId),
            cancellationToken);
        return run is null ? NotFound() : Ok(run);
    }

    [HttpGet("hangfire")]
    public async Task<IActionResult> GetHangfireOverview(
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetHangfireOverviewQuery(), cancellationToken));

    [HttpGet("hangfire/job-details/{jobId}")]
    public async Task<IActionResult> GetHangfireJobDetails(
        string jobId,
        CancellationToken cancellationToken)
    {
        var details = await sender.Send(
            new GetHangfireJobDetailsQuery(jobId),
            cancellationToken);
        return details is null ? NotFound() : Ok(details);
    }

    [HttpGet("hangfire/jobs/{state}")]
    public async Task<IActionResult> GetHangfireJobs(
        string state,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] int? from = null,
        [FromQuery] int? count = null,
        [FromQuery] string? queue = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetHangfireJobsQuery(state, page, pageSize, from, count, queue),
            cancellationToken);
        return result is null
            ? BadRequest($"Unknown Hangfire state '{state}'.")
            : Ok(result);
    }

    [HttpGet("hangfire/retries")]
    public async Task<IActionResult> GetHangfireRetries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(
            new GetHangfireRetriesQuery(page, pageSize),
            cancellationToken));

    [HttpGet("hangfire/recurring")]
    public async Task<IActionResult> GetRecurringJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await sender.Send(
            new GetHangfireRecurringJobsQuery(page, pageSize),
            cancellationToken));

    [HttpPost("hangfire/recurring/{jobId}/trigger")]
    public async Task<IActionResult> TriggerRecurringJob(
        string jobId,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new TriggerHangfireRecurringJobCommand(jobId),
            cancellationToken);
        return Accepted(new { jobId, message = "Recurring job đã được kích hoạt." });
    }

    [HttpDelete("hangfire/recurring/{jobId}")]
    public async Task<IActionResult> DeleteRecurringJob(
        string jobId,
        CancellationToken cancellationToken) =>
        await sender.Send(
            new DeleteHangfireRecurringJobCommand(jobId),
            cancellationToken)
            ? NoContent()
            : NotFound();

    [HttpPost("hangfire/jobs/{jobId}/requeue")]
    public async Task<IActionResult> RequeueJob(
        string jobId,
        CancellationToken cancellationToken) =>
        await sender.Send(
            new RequeueHangfireJobCommand(jobId),
            cancellationToken)
            ? Accepted(new { jobId, message = "Job đã được đưa lại vào hàng đợi." })
            : NotFound();

    [HttpDelete("hangfire/jobs/{jobId}")]
    public async Task<IActionResult> DeleteJob(
        string jobId,
        CancellationToken cancellationToken) =>
        await sender.Send(
            new DeleteHangfireJobCommand(jobId),
            cancellationToken)
            ? NoContent()
            : NotFound();
}
