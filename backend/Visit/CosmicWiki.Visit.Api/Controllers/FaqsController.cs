using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Visit.Application.Features.Faqs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Visit.Api.Controllers;

[ApiController]
[Route("api/visit/faqs")]
public sealed class FaqsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetFaqsQuery query,
        CancellationToken cancellationToken) =>
        Ok(await sender.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFaqByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFaqCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFaqRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateFaqCommand(
            id, request.Question, request.Answer, request.DisplayOrder, request.IsPublished),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.VisitWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteFaqCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdateFaqRequest(
    string Question,
    string Answer,
    int DisplayOrder,
    bool IsPublished);
