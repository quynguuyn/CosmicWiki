using CosmicWiki.Visit.Application.Features.CreateVisitRequest;
using CosmicWiki.Visit.Application.Features.ChangeVisitRequestStatus;
using CosmicWiki.Visit.Application.Features.GetVisitRequest;
using MediatR;
using CosmicWiki.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Visit.Api.Controllers;
[ApiController]
[Route("api/visit/requests")]
public sealed class VisitRequestsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateVisitRequestCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        var request = await sender.Send(new GetVisitRequestQuery(id, null), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, request);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.VisitRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetVisitRequestQuery(id, null), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("reference/{referenceCode}")]
    [Authorize(Policy = Permissions.VisitRead)]
    public async Task<IActionResult> GetByReference(string referenceCode, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetVisitRequestQuery(null, referenceCode), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ConfirmVisitRequestCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/decline")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Decline(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeclineVisitRequestCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelVisitRequestCommand(id), cancellationToken);
        return NoContent();
    }
}
