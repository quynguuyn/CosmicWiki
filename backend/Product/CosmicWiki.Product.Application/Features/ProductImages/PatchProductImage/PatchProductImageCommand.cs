using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.PatchProductImage;

public sealed record PatchProductImageCommand(
    Guid ProductId,
    Guid ImageId,
    string? Url,
    int? DisplayOrder
) : IRequest;