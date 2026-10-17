using CosmicWiki.Product.Application.Features.Products.Common;
using CosmicWiki.Pagination;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.GetProducts;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? CategoryId = null
) : IRequest<PagedResult<ProductResponse>>;