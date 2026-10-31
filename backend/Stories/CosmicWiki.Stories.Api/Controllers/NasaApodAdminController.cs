using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Stories.Infrastructure.Research;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Stories.Api.Controllers;

[ApiController]
[Route("api/stories/admin/apod")]
[Authorize(Policy = Permissions.StoryWrite)]
public sealed class NasaApodAdminController(
    INasaApodProvider provider,
    IBackgroundJobClient jobs) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken) =>
        Ok(await provider.GetStatusAsync(cancellationToken));

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger(CancellationToken cancellationToken)
    {
        var status = await provider.GetStatusAsync(cancellationToken);
        if (!status.IsReachable)
            return Conflict(status);

        return Accepted(new
        {
            jobId = jobs.Enqueue<NasaApodStoryJob>(
                job => job.RunAsync(CancellationToken.None))
        });
    }
}
