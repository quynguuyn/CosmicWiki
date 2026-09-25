using CosmicWiki.Product.Application.Features.Products.Common;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.GetProductById;

public sealed record GetProductByIdQuery(
    Guid Id
) : IRequest<ProductResponse?>;