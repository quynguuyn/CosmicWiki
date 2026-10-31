using CosmicWiki.Product.Application.Features.Categories.CreateCategory;
using CosmicWiki.Product.Application.Features.Categories.DeleteCategory;
using CosmicWiki.Product.Application.Features.Categories.GetCategories;
using CosmicWiki.Product.Application.Features.Categories.GetCategoryById;
using CosmicWiki.Product.Application.Features.Categories.PatchCategory;
using CosmicWiki.Product.Application.Features.Categories.UpdateCategory;
using CosmicWiki.SharedKernel.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Product.Api.Controllers;

[ApiController]
[Route("api/product/categories")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCategoriesQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("page")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetCategoriesPageQuery(page, pageSize, search),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCategoryByIdQuery(id),
            cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryCommand command,
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
    [FromBody] UpdateCategoryRequest request,
    CancellationToken cancellationToken)
{
    await _sender.Send(
        new UpdateCategoryCommand(
            id,
            request.Name,
            request.Slug,
            request.Description),
        cancellationToken);

    return NoContent();
}

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Permissions.ProductWrite)]
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] PatchCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PatchCategoryCommand(
                id,
                request.Name,
                request.Slug,
                request.Description),
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
            new DeleteCategoryCommand(id),
            cancellationToken);

        return NoContent();
    }
}
