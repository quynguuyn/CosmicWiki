using CosmicWiki.Product.Domain.Enums;

namespace CosmicWiki.Product.Application.Features.Products.Common;

public sealed record ProductResponse(
    Guid Id,
    string SKU,
    string Name,
    string Slug,
    string? Description,
    decimal OriginalPrice,
    decimal SalePrice,
    int StockQuantity,
    ProductStatus Status,
    Guid CategoryId,
    IReadOnlyCollection<ProductImageResponse> Images,
    IReadOnlyCollection<string>? Urls = null);