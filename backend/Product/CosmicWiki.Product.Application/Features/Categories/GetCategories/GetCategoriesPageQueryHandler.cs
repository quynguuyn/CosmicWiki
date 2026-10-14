using CosmicWiki.Pagination;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.GetCategories;

public sealed class GetCategoriesPageQueryHandler(
    ICategoryRepository repository)
    : IRequestHandler<GetCategoriesPageQuery, PagedResult<CategoryResponse>>
{
    public async Task<PagedResult<CategoryResponse>> Handle(
        GetCategoriesPageQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            throw new ArgumentException("Page must be greater than zero.", nameof(request.Page));
        if (request.PageSize < 1 || request.PageSize > PaginationOptions.MaxPageSize)
            throw new ArgumentException(
                $"Page size must be between 1 and {PaginationOptions.MaxPageSize}.",
                nameof(request.PageSize));

        var totalCount = await repository.CountAsync(request.Search, cancellationToken);
        var categories = await repository.GetPageAsync(
            (request.Page - 1) * request.PageSize,
            request.PageSize,
            request.Search,
            cancellationToken);
        var items = categories.Select(category => new CategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.Description)).ToArray();

        return new PagedResult<CategoryResponse>(
            items, request.Page, request.PageSize, totalCount);
    }
}
