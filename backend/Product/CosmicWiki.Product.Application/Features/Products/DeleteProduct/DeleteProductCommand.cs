using MediatR;

namespace CosmicWiki.Product.Application.Features.Products.DeleteProduct;

public sealed record DeleteProductCommand(
    Guid Id
) : IRequest;