namespace CosmicWiki.Product.Application.Features.ProductImages.Common;

public sealed record ProductImageResponse(
    Guid Id,
    Guid ProductId,
    string Url,
    int DisplayOrder);