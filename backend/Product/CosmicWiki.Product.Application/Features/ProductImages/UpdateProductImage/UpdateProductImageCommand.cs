using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.UpdateProductImage;

public sealed record UpdateProductImageCommand(
    Guid ProductId,
    Guid ImageId,
    string Url,
    int DisplayOrder
) : IRequest;