namespace CosmicWiki.Product.Application.Features.ProductImages.CreateProductImage;

public sealed record CreateProductImageRequest(
    string Url,
    int DisplayOrder = 0);