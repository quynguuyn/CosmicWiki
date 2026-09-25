using CosmicWiki.Product.Domain.Enums;

namespace CosmicWiki.Product.Application.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    string SKU,
    string Name,
    string Slug,
    string? Description,
    decimal OriginalPrice,
    decimal SalePrice,
    int StockQuantity,
    ProductStatus Status,
    Guid CategoryId);