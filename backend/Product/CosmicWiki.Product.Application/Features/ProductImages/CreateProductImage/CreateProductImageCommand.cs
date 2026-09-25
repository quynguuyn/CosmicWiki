using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.CreateProductImage;

public sealed record CreateProductImageCommand(
    Guid ProductId,
    string Url,
    int DisplayOrder = 0
) : IRequest<Guid>;