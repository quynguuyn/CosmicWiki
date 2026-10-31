using CosmicWiki.Product.Application.Features.ProductImages.CreateProductImage;
using CosmicWiki.Product.Application.Features.ProductImages.DeleteProductImage;
using CosmicWiki.Product.Application.Features.ProductImages.GetProductImageById;
using CosmicWiki.Product.Application.Features.ProductImages.GetProductImages;
using CosmicWiki.Product.Application.Features.ProductImages.PatchProductImage;
using CosmicWiki.Product.Application.Features.ProductImages.UpdateProductImage;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Product.Api.Controllers;

[ApiController]
[Route("api/product/products/{productId:guid}/images")]
[Authorize]
public sealed class ProductImagesController : ControllerBase
{
    private readonly ISender _sender;

    public ProductImagesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetProductImagesQuery(productId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{imageId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetProductImageByIdQuery(
                productId,
                imageId),
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Create(
        Guid productId,
        [FromBody] CreateProductImageRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            new CreateProductImageCommand(
                productId,
                request.Url,
                request.DisplayOrder),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                productId,
                imageId = id
            },
            id);
    }

    [HttpPut("{imageId:guid}")]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Update(
        Guid productId,
        Guid imageId,
        [FromBody] UpdateProductImageRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateProductImageCommand(
                productId,
                imageId,
                request.Url,
                request.DisplayOrder),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{imageId:guid}")]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Patch(
        Guid productId,
        Guid imageId,
        [FromBody] PatchProductImageRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PatchProductImageCommand(
                productId,
                imageId,
                request.Url,
                request.DisplayOrder),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{imageId:guid}")]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Delete(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteProductImageCommand(
                productId,
                imageId),
            cancellationToken);

        return NoContent();
    }
}
