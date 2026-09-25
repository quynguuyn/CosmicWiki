namespace CosmicWiki.Product.Application.Features.ProductImages.UpdateProductImage;

public sealed record UpdateProductImageRequest(
    string Url,
    int DisplayOrder);