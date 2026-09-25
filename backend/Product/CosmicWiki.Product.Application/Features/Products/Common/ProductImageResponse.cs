namespace CosmicWiki.Product.Application.Features.Products.Common;

public sealed record ProductImageResponse(
    Guid Id,
    string Url,
    int DisplayOrder);