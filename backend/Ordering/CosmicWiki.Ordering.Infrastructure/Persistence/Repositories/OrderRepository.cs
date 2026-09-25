using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Domain.Entities;
using CosmicWiki.Payments.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Ordering.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(
    OrderingDbContext dbContext,
    IInventoryReservationService inventory) :
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
        dbContext.Orders.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Add(Order order) => dbContext.Orders.Add(order);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> MarkPaidAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetAsync(orderId, cancellationToken) ?? throw new KeyNotFoundException($"Order '{orderId}' was not found.");
        if (order.Status == Domain.Enums.OrderStatus.Paid)
            return true;
        if (!await inventory.ConfirmAsync(orderId, cancellationToken))
            return false;
        order.MarkPaid();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task MarkPaymentFailedAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetAsync(orderId, cancellationToken) ?? throw new KeyNotFoundException($"Order '{orderId}' was not found.");
        order.MarkPaymentFailed();
        await inventory.ReleaseAsync(orderId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
