using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Visit.Application.Features.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Visit.Api.Controllers;

[ApiController]
[Route("api/visit/events")]
public sealed class EventsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetEventsQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEventByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateEventCommand(
            id, request.EventDateUtc, request.Name, request.Description,
            request.Specification, request.Location, request.Status, request.StatusType,
            request.Action, request.DisplayOrder, request.IsPublished),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteEventCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateEventRequest(
    DateTime EventDateUtc,
    string Name,
    string Description,
    string? Specification,
    string? Location,
    string Status,
    string? StatusType,
    string? Action,
    int DisplayOrder,
    bool IsPublished);
