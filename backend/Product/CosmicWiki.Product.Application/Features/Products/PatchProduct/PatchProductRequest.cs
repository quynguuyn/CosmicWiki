using CosmicWiki.Product.Domain.Enums;

namespace CosmicWiki.Product.Application.Features.Products.PatchProduct;

public sealed record PatchProductRequest(
    string? SKU,
    string? Name,
    string? Slug,
    string? Description,
    decimal? OriginalPrice,
    decimal? SalePrice,
    int? StockQuantity,
    ProductStatus? Status,
    Guid? CategoryId);