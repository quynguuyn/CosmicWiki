using CosmicWiki.Product.Application.Features.Products.CreateProduct;
using CosmicWiki.Product.Application.Features.Products.DeleteProduct;
using CosmicWiki.Product.Application.Features.Products.GetProductById;
using CosmicWiki.Product.Application.Features.Products.GetProducts;
using CosmicWiki.Product.Application.Features.Products.PatchProduct;
using CosmicWiki.Product.Application.Features.Products.UpdateProduct;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Product.Api.Controllers;

[ApiController]
[Route("api/product/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.ProductRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetProductsQuery(page, pageSize, search, categoryId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.ProductRead)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetProductByIdQuery(id),
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateProductCommand(
                id,
                request.SKU,
                request.Name,
                request.Slug,
                request.Description,
                request.OriginalPrice,
                request.SalePrice,
                request.StockQuantity,
                request.Status,
                request.CategoryId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] PatchProductRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PatchProductCommand(
                id,
                request.SKU,
                request.Name,
                request.Slug,
                request.Description,
                request.OriginalPrice,
                request.SalePrice,
                request.StockQuantity,
                request.Status,
                request.CategoryId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteProductCommand(id),
            cancellationToken);

        return NoContent();
    }
}