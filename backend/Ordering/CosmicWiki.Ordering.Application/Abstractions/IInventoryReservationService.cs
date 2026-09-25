namespace CosmicWiki.Ordering.Application.Abstractions;

public interface IInventoryReservationService
{
    Task<bool> ReserveAsync(
        Guid orderId,
        IReadOnlyCollection<InventoryReservationLine> lines,
        CancellationToken cancellationToken);

    Task<bool> ConfirmAsync(Guid orderId, CancellationToken cancellationToken);

    Task ReleaseAsync(Guid orderId, CancellationToken cancellationToken);

    Task<int> ReleaseExpiredAsync(CancellationToken cancellationToken);
}

public sealed record InventoryReservationLine(Guid ProductId, int Quantity);
