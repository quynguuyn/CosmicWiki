using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Stories.Application.Features.NasaApod;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Stories.Api.Controllers;

[ApiController]
[Route("api/stories/admin/apod")]
[Authorize(Policy = Permissions.StoryWrite)]
public sealed class NasaApodAdminController(ISender sender) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetNasaApodStatusQuery(), cancellationToken));

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new TriggerNasaApodStoryCommand(), cancellationToken);
        if (result.JobId is null)
            return Conflict(result.Status);

        return Accepted(new
        {
            jobId = result.JobId
        });
    }
}
