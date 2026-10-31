using CosmicWiki.Catalog.Application.Features.AstronomicalImages.Add;
using CosmicWiki.Catalog.Application.Features.AstronomicalImages.Delete;
using CosmicWiki.Catalog.Application.Features.AstronomicalImages.GetByAstronomicalObject;
using CosmicWiki.Catalog.Application.Features.AstronomicalImages.GetById;
using CosmicWiki.Catalog.Application.Features.AstronomicalImages.Patch;
using CosmicWiki.Catalog.Application.Features.AstronomicalImages.Update;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/astronomical-objects/{astronomicalObjectId:guid}/images")]
[Authorize]
public sealed class AstronomicalImagesController : ControllerBase
{
    private readonly ISender _sender;

    public AstronomicalImagesController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
        Guid astronomicalObjectId,
        CancellationToken cancellationToken)
    {
        var images = await _sender.Send(
            new GetAstronomicalImagesQuery(
                astronomicalObjectId),
            cancellationToken);

        return Ok(images);
    }

    [HttpGet("{imageId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid astronomicalObjectId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var image = await _sender.Send(
            new GetAstronomicalImageByIdQuery(
                astronomicalObjectId,
                imageId),
            cancellationToken);

        if (image is null)
            return NotFound();

        return Ok(image);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Add(
        Guid astronomicalObjectId,
        [FromBody] AddAstronomicalImageCommand command,
        CancellationToken cancellationToken)
    {
        var imageId = await _sender.Send(
            command with
            {
                AstronomicalObjectId = astronomicalObjectId
            },
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                astronomicalObjectId,
                imageId
            },
            new
            {
                id = imageId
            });
    }

    [HttpPut("{imageId:guid}")]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Update(
        Guid astronomicalObjectId,
        Guid imageId,
        [FromBody] UpdateAstronomicalImageRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateAstronomicalImageCommand(
                astronomicalObjectId,
                imageId,
                request.Url,
                request.Urls),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{imageId:guid}")]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Patch(
        Guid astronomicalObjectId,
        Guid imageId,
        [FromBody] PatchAstronomicalImageRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PatchAstronomicalImageCommand(
                astronomicalObjectId,
                imageId,
                request.Url,
                request.Urls),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{imageId:guid}")]
    [Authorize(Policy = Permissions.CatalogWrite)]
    public async Task<IActionResult> Delete(
        Guid astronomicalObjectId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteAstronomicalImageCommand(
                astronomicalObjectId,
                imageId),
            cancellationToken);

        return NoContent();
    }
}
