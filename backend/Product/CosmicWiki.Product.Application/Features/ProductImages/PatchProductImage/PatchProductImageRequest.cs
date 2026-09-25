namespace CosmicWiki.Product.Application.Features.ProductImages.PatchProductImage;

public sealed record PatchProductImageRequest(
    string? Url,
    int? DisplayOrder);