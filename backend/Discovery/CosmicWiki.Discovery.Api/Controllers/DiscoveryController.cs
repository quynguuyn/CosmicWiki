using CosmicWiki.Discovery.Application.Features.Search;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Discovery.Api.Controllers;

[ApiController]
[Route("api/discovery")]
[Authorize]
public sealed class DiscoveryController : ControllerBase
{
    private readonly ISender _sender;

    public DiscoveryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("search")]
    [Authorize(Policy = Permissions.DiscoveryRead)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search,
        [FromQuery] string? sourceType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new SearchQuery(
                search,
                sourceType,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }
}