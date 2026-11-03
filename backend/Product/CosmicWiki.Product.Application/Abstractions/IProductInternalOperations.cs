using CosmicWiki.Product.Application.Features.InternalProduct;

namespace CosmicWiki.Product.Application.Abstractions;

public interface IProductInternalOperations
{
    Task<ProductSnapshotResult?> GetSnapshotAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<bool> ReserveAsync(
        Guid orderId,
        IReadOnlyCollection<ReservationLine> lines,
        CancellationToken cancellationToken);

    Task<bool> ConfirmAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task ReleaseAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<int> ReleaseExpiredAsync(CancellationToken cancellationToken);
}
