using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Repositories;

public sealed class OrderNotificationRepository(OrderingDbContext db) : IOrderNotificationRepository
{
    public async Task<IReadOnlyCollection<Guid>> GetRecentOrderIdsAsync(
        Guid customerId,
        int take,
        CancellationToken cancellationToken) =>
        await db.Orders
            .AsNoTracking()
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => order.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<OrderNotificationHistory>> GetStatusHistoryAsync(
        IReadOnlyCollection<Guid> orderIds,
        CancellationToken cancellationToken) =>
        await db.OrderStatusHistory
            .AsNoTracking()
            .Where(history => orderIds.Contains(history.OrderId))
            .Select(history => new OrderNotificationHistory(
                history.OrderId,
                history.Status,
                history.ChangedAtUtc))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<(Guid OrderId, string Status)>> GetExistingOrderStatusesAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var existing = await db.OrderNotifications
            .Where(item => item.CustomerId == customerId)
            .Select(item => new { item.OrderId, item.Status })
            .ToListAsync(cancellationToken);
        return existing.Select(item => (item.OrderId, item.Status)).ToArray();
    }

    public async Task<IReadOnlyCollection<OrderNotification>> GetByCustomerIdAsync(
        Guid customerId,
        int take,
        CancellationToken cancellationToken) =>
        await db.OrderNotifications
            .AsNoTracking()
            .Where(item => item.CustomerId == customerId)
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountUnreadByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken) =>
        db.OrderNotifications.CountAsync(
            item => item.CustomerId == customerId && item.ReadAtUtc == null,
            cancellationToken);

    public Task<OrderNotification?> GetAsync(
        Guid notificationId,
        Guid customerId,
        CancellationToken cancellationToken) =>
        db.OrderNotifications.SingleOrDefaultAsync(
            item => item.Id == notificationId && item.CustomerId == customerId,
            cancellationToken);

    public async Task<IReadOnlyCollection<OrderNotification>> GetUnreadByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken) =>
        await db.OrderNotifications
            .Where(item => item.CustomerId == customerId && item.ReadAtUtc == null)
            .ToListAsync(cancellationToken);

    public void Add(OrderNotification notification) => db.OrderNotifications.Add(notification);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
