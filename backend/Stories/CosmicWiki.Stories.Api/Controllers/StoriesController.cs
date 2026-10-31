using CosmicWiki.SharedKernel.Authorization;
using CosmicWiki.Stories.Application.Features.Stories.CreateStory;
using CosmicWiki.Stories.Application.Features.Stories.DeleteStory;
using CosmicWiki.Stories.Application.Features.Stories.GetCategories;
using CosmicWiki.Stories.Application.Features.Stories.GetStories;
using CosmicWiki.Stories.Application.Features.Stories.GetStoryById;
using CosmicWiki.Stories.Application.Features.Stories.GetStoryBySlug;
using CosmicWiki.Stories.Application.Features.Stories.PatchStory;
using CosmicWiki.Stories.Application.Features.Stories.UpdateStory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Stories.Api.Controllers;

[ApiController]
[Route("api/stories")]
[Authorize]
public sealed class StoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "newest",
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetStoriesQuery(search, category, page, pageSize, sort),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoryCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoryBySlugQuery(slug), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoryByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.StoryWrite)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStoryCommand command,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.StoryWrite)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateStoryRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateStoryCommand(
                id,
                request.Category,
                request.Era,
                request.Code,
                request.Title,
                request.Slug,
                request.Description,
                request.Content,
                request.Author,
                request.ReadTimeMinutes,
                request.ImageUrl,
                request.ImageAlt,
                request.Status),
            cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = Permissions.StoryWrite)]
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] PatchStoryRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new PatchStoryCommand(
                id,
                request.Category,
                request.Era,
                request.Code,
                request.Title,
                request.Slug,
                request.Description,
                request.Content,
                request.Author,
                request.ReadTimeMinutes,
                request.ImageUrl,
                request.ImageAlt,
                request.Status),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.StoryWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteStoryCommand(id), cancellationToken);
        return NoContent();
    }
}
