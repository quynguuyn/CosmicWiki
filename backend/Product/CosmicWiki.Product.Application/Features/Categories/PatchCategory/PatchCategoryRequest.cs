namespace CosmicWiki.Product.Application.Features.Categories.PatchCategory;

public sealed record PatchCategoryRequest(
    string? Name,
    string? Slug,
    string? Description);