using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.GetCategories;

public sealed record GetCategoriesQuery
    : IRequest<IReadOnlyCollection<CategoryResponse>>;
    
public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description);