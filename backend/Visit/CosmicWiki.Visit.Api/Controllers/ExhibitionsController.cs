using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Visit.Application.Features.Exhibitions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Visit.Api.Controllers;

[ApiController]
[Route("api/visit/exhibitions")]
public sealed class ExhibitionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetExhibitionsQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetExhibitionByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExhibitionCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateExhibitionRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateExhibitionCommand(
            id, request.Number, request.Category, request.Title, request.Description,
            request.Status, request.ImageUrl, request.DisplayOrder, request.IsPublished),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteExhibitionCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateExhibitionRequest(
    string Number,
    string Category,
    string Title,
    string Description,
    string Status,
    string? ImageUrl,
    int DisplayOrder,
    bool IsPublished);
