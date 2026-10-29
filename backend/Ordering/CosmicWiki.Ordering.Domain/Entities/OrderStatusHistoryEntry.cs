using CosmicWiki.Ordering.Domain.Enums;

namespace CosmicWiki.Ordering.Domain.Entities;

public sealed class OrderStatusHistoryEntry
{
    private OrderStatusHistoryEntry() { }

    private OrderStatusHistoryEntry(
        Guid id,
        Guid orderId,
        OrderStatus status,
        DateTime changedAtUtc)
    {
        Id = id;
        OrderId = orderId;
        Status = status;
        ChangedAtUtc = changedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }

    internal static OrderStatusHistoryEntry Create(
        Guid orderId,
        OrderStatus status,
        DateTime changedAtUtc) =>
        new(Guid.NewGuid(), orderId, status, changedAtUtc);
}
