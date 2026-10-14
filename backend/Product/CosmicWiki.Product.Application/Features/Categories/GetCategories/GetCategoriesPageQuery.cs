using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.GetCategories;

public sealed record GetCategoriesPageQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null) : IRequest<PagedResult<CategoryResponse>>;
