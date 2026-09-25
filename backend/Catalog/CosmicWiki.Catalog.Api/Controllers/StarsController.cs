using CosmicWiki.Catalog.Application.Features.Stars.GetAll;
using CosmicWiki.Catalog.Application.Features.Stars.Create;
using CosmicWiki.Catalog.Application.Features.Stars.Delete;
using CosmicWiki.Catalog.Application.Features.Stars.GetById;
using CosmicWiki.Catalog.Application.Features.Stars.Patch;
using CosmicWiki.Catalog.Application.Features.Stars.Update;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/stars")]
[Authorize]
public sealed class StarsController : ControllerBase
{
    private readonly ISender _sender;

    public StarsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetAllStarsQuery(page, pageSize),
            cancellationToken);

        return Ok(result);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogRead)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var star = await _sender.Send(
            new GetStarByIdQuery(id),
            cancellationToken);

        if (star is null)
            return NotFound();

        return Ok(star);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStarCommand command,
        CancellationToken cancellationToken)
    {
        var starId = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = starId },
            new { id = starId });
    }
    
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateStarRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateStarCommand(
                id, 
                request.Name, 
                request.CatalogIdentifiers, 
                request.Parallaxes),
            cancellationToken);

        return NoContent();
    }
    
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] PatchStarRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PatchStarCommand(
                id, 
                request.Name, 
                request.CatalogIdentifiers, 
                request.Parallaxes),
            cancellationToken);

        return NoContent();
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteStarCommand(id),
            cancellationToken);

        return NoContent();
    }
}