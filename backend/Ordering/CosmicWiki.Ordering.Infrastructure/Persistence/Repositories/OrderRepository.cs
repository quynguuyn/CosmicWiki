using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.Payments.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(
    OrderingDbContext dbContext,
    IInventoryReservationService inventory,
    IOrderNotificationPublisher notificationPublisher) :
    IOrderRepository,
    IOrderPaymentStatusUpdater,
    IOrderPaymentValidator,
    IOrderPaymentIntentValidator
{
    public async Task<bool> IsPaymentIntentValidAsync(Guid orderId, string paymentIntentId, decimal amount, string currency, CancellationToken cancellationToken)
    {
        var order = await GetAsync(orderId, cancellationToken);
        return order is not null
            && string.Equals(order.PaymentIntentId, paymentIntentId, StringComparison.Ordinal)
            && order.TotalAmount == amount
            && string.Equals(order.Currency, currency, StringComparison.OrdinalIgnoreCase);
    }

    public Task<Order?> GetByIdempotencyKeyAsync(string key, CancellationToken cancellationToken) =>
        dbContext.Orders.Include(x => x.Lines).SingleOrDefaultAsync(x => x.IdempotencyKey == key, cancellationToken);

    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Orders
            .Include(x => x.Lines)
            .Include(x => x.StatusHistory)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken) =>
        await dbContext.Orders
            .Include(x => x.Lines)
            .Include(x => x.StatusHistory)
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetAvailableForDeliveryAsync(CancellationToken cancellationToken) =>
        await dbContext.Orders
            .Include(x => x.Lines)
            .Where(x => x.Status == Domain.Enums.OrderStatus.Confirmed && x.DeliveryPersonId == null)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetByDeliveryPersonIdAsync(Guid deliveryPersonId, CancellationToken cancellationToken) =>
        await dbContext.Orders
            .Include(x => x.Lines)
            .Where(x => x.DeliveryPersonId == deliveryPersonId && x.Status == Domain.Enums.OrderStatus.Shipping)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetCreatedSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken) =>
        await dbContext.Orders
            .Include(x => x.Lines)
            .Where(x => x.CreatedAtUtc >= sinceUtc)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyCollection<Order> Items, int TotalCount)> GetCreatedSincePageAsync(
        DateTime sinceUtc,
        Domain.Enums.OrderStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Orders
            .Include(x => x.Lines)
            .Where(x => x.CreatedAtUtc >= sinceUtc);
        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Order order) => dbContext.Orders.Add(order);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> MarkPaidAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetAsync(orderId, cancellationToken) ?? throw new KeyNotFoundException($"Order '{orderId}' was not found.");
        if (order.Status is OrderStatus.Paid or OrderStatus.Confirmed or OrderStatus.Shipping)
            return true;
        if (!await inventory.ConfirmAsync(orderId, cancellationToken))
            return false;
        var previousStatus = order.Status;
        order.MarkPaid();
        await dbContext.SaveChangesAsync(cancellationToken);
        if (previousStatus != order.Status)
            await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
        return true;
    }

    public async Task MarkPaymentFailedAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetAsync(orderId, cancellationToken) ?? throw new KeyNotFoundException($"Order '{orderId}' was not found.");
        var previousStatus = order.Status;
        order.MarkPaymentFailed();
        await inventory.ReleaseAsync(orderId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        if (previousStatus != order.Status)
            await notificationPublisher.PublishStatusChangedAsync(order, cancellationToken);
    }
}
