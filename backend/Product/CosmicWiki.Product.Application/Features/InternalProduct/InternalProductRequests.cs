using CosmicWiki.Product.Application.Abstractions;
using MediatR;

namespace CosmicWiki.Product.Application.Features.InternalProduct;

public sealed record ProductSnapshotResult(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice,
    string? ImageUrl,
    int AvailableStock,
    bool IsActive);

public sealed record ReservationLine(Guid ProductId, int Quantity);

public sealed record GetProductSnapshotQuery(Guid ProductId)
    : IRequest<ProductSnapshotResult?>;

public sealed record ReserveProductInventoryCommand(
    Guid OrderId,
    IReadOnlyCollection<ReservationLine> Lines) : IRequest<bool>;

public sealed record ConfirmProductInventoryCommand(Guid OrderId) : IRequest<bool>;

public sealed record ReleaseProductInventoryCommand(Guid OrderId) : IRequest<Unit>;

public sealed record ReleaseExpiredProductInventoryCommand : IRequest<int>;

public sealed class GetProductSnapshotHandler(
    IProductInternalOperations operations)
    : IRequestHandler<GetProductSnapshotQuery, ProductSnapshotResult?>
{
    public Task<ProductSnapshotResult?> Handle(
        GetProductSnapshotQuery request,
        CancellationToken cancellationToken) =>
        operations.GetSnapshotAsync(request.ProductId, cancellationToken);
}

public sealed class ReserveProductInventoryHandler(
    IProductInternalOperations operations)
    : IRequestHandler<ReserveProductInventoryCommand, bool>
{
    public Task<bool> Handle(
        ReserveProductInventoryCommand request,
        CancellationToken cancellationToken) =>
        operations.ReserveAsync(
            request.OrderId,
            request.Lines,
            cancellationToken);
}

public sealed class ConfirmProductInventoryHandler(
    IProductInternalOperations operations)
    : IRequestHandler<ConfirmProductInventoryCommand, bool>
{
    public Task<bool> Handle(
        ConfirmProductInventoryCommand request,
        CancellationToken cancellationToken) =>
        operations.ConfirmAsync(request.OrderId, cancellationToken);
}

public sealed class ReleaseProductInventoryHandler(
    IProductInternalOperations operations)
    : IRequestHandler<ReleaseProductInventoryCommand, Unit>
{
    public async Task<Unit> Handle(
        ReleaseProductInventoryCommand request,
        CancellationToken cancellationToken)
    {
        await operations.ReleaseAsync(request.OrderId, cancellationToken);
        return Unit.Value;
    }
}

public sealed class ReleaseExpiredProductInventoryHandler(
    IProductInternalOperations operations)
    : IRequestHandler<ReleaseExpiredProductInventoryCommand, int>
{
    public Task<int> Handle(
        ReleaseExpiredProductInventoryCommand request,
        CancellationToken cancellationToken) =>
        operations.ReleaseExpiredAsync(cancellationToken);
}
