using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.DeleteProductImage;

public sealed record DeleteProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : IRequest;