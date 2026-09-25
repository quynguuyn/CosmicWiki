using CosmicWiki.Product.Application.Features.ProductImages.Common;
using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.GetProductImageById;

public sealed record GetProductImageByIdQuery(
    Guid ProductId,
    Guid ImageId
) : IRequest<ProductImageResponse?>;