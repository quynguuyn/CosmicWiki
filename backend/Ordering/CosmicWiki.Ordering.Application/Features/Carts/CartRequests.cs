using CosmicWiki.Ordering.Application.Abstractions;
using MediatR;

namespace CosmicWiki.Ordering.Application.Features.Carts;

public sealed record GetCartQuery(string CustomerId) : IRequest<string?>;

public sealed class GetCartQueryHandler(ICartRepository carts)
    : IRequestHandler<GetCartQuery, string?>
{
    public Task<string?> Handle(GetCartQuery request, CancellationToken cancellationToken) =>
        carts.GetCartAsync(request.CustomerId, cancellationToken);
}

public sealed record SaveCartCommand(string CustomerId, string CartJson) : IRequest;

public sealed class SaveCartCommandHandler(ICartRepository carts)
    : IRequestHandler<SaveCartCommand>
{
    public Task Handle(SaveCartCommand request, CancellationToken cancellationToken) =>
        carts.SaveCartAsync(request.CustomerId, request.CartJson, cancellationToken);
}

public sealed record ClearCartCommand(string CustomerId) : IRequest;

public sealed class ClearCartCommandHandler(ICartRepository carts)
    : IRequestHandler<ClearCartCommand>
{
    public Task Handle(ClearCartCommand request, CancellationToken cancellationToken) =>
        carts.DeleteCartAsync(request.CustomerId, cancellationToken);
}
