using CosmicWiki.Product.Domain.Enums;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string SKU,
    string Name,
    string Slug,
    string? Description,
    decimal OriginalPrice,
    decimal SalePrice,
    int StockQuantity,
    ProductStatus Status,
    Guid CategoryId
) : IRequest;