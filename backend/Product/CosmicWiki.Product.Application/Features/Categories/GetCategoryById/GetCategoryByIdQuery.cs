using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.GetCategoryById;

public sealed record GetCategoryByIdQuery(
    Guid Id ) : IRequest<CategoryResponse?>;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description);