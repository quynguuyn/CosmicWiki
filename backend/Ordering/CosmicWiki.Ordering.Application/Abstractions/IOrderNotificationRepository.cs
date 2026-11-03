using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;

namespace CosmicWiki.Ordering.Application.Abstractions;

public sealed record OrderNotificationHistory(
    Guid OrderId,
    OrderStatus Status,
    DateTime ChangedAtUtc);

public interface IOrderNotificationRepository
{
    Task<IReadOnlyCollection<Guid>> GetRecentOrderIdsAsync(
        Guid customerId,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OrderNotificationHistory>> GetStatusHistoryAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<(Guid OrderId, string Status)>> GetExistingOrderStatusesAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OrderNotification>> GetByCustomerIdAsync(
        Guid customerId,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountUnreadByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);

    Task<OrderNotification?> GetAsync(
        Guid notificationId,
        Guid customerId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OrderNotification>> GetUnreadByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    void Add(OrderNotification notification);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
