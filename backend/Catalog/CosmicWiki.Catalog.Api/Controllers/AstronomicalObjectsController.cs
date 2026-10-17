using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;
using CosmicWiki.Pagination;
using CosmicWiki.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/objects")]
[Authorize(Policy = Permissions.CatalogRead)]
public sealed class AstronomicalObjectsController(
    IAstronomicalObjectRepository objectRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] AstronomicalObjectKind? kind = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequest("Page must be greater than or equal to 1.");

        pageSize = Math.Clamp(pageSize, 1, PaginationOptions.MaxPageSize);
        var options = new PaginationOptions(page, pageSize);
        var result = await objectRepository.GetPagedAsync(
            options.Skip,
            options.PageSize,
            kind,
            cancellationToken);
        var items = result.Items
            .Select(ToDto)
            .ToArray();

        return Ok(new PagedResult<AstronomicalObjectDto>(
            items,
            options.Page,
            options.PageSize,
            result.TotalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await objectRepository.GetReadModelByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(ToDto(item));
    }

    private static AstronomicalObjectDto ToDto(AstronomicalObjectReadModel item) =>
        new(
            item.Id,
            item.Name,
            item.Kind.ToString(),
            item.ObjectType,
            item.Catalog,
            item.CatalogValue,
            item.RightAscensionDegrees,
            item.DeclinationDegrees,
            item.ImageUrl);

    private sealed record AstronomicalObjectDto(
        Guid Id,
        string? Name,
        string Kind,
        string? ObjectType,
        string? Catalog,
        string? CatalogValue,
        double? RightAscensionDegrees,
        double? DeclinationDegrees,
        string? ImageUrl);
}
