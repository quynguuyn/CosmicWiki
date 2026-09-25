using CosmicWiki.Product.Domain.Enums;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.CreateProduct;

public sealed record CreateProductCommand(
    string SKU,
    string Name,
    string Slug,
    string? Description,
    decimal OriginalPrice,
    decimal SalePrice,
    int StockQuantity,
    Guid CategoryId,
    ProductStatus Status = ProductStatus.Draft,
    IReadOnlyCollection<CreateProductImage> Images = null
) : IRequest<Guid>;

public sealed record CreateProductImage(
    string Url,
    int DisplayOrder);