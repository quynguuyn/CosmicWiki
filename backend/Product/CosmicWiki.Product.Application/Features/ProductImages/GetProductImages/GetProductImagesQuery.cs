using CosmicWiki.Product.Application.Features.ProductImages.Common;
using MediatR;

namespace CosmicWiki.Product.Application.Features.ProductImages.GetProductImages;

public sealed record GetProductImagesQuery(
    Guid ProductId
) : IRequest<IReadOnlyCollection<ProductImageResponse>>;