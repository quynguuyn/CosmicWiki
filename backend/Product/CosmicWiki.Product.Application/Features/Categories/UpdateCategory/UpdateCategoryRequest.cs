namespace CosmicWiki.Product.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryRequest(
    string Name,
    string Slug,
    string? Description);