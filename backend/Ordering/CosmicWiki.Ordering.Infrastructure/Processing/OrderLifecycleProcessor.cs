using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Ordering.Infrastructure.Processing;

public sealed class OrderLifecycleProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<OrderLifecycleOptions> options,
    ILogger<OrderLifecycleProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
                var now = DateTime.UtcNow;
                var timeout = now.AddMinutes(-Math.Max(1, options.Value.PendingPaymentTimeoutMinutes));
                var expiredOrderIds = await db.Orders
                    .AsNoTracking()
                    .Where(x => x.Status == OrderStatus.PendingPayment && x.CreatedAtUtc <= timeout)
                    .Select(x => x.Id)
                    .ToListAsync(stoppingToken);

                foreach (var orderId in expiredOrderIds)
                    await CancelExpiredOrderAsync(orderId, timeout, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process automatic order lifecycle transitions.");
            }
        }
    }

    private async Task CancelExpiredOrderAsync(
        Guid orderId,
        DateTime timeout,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var orders = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var order = await orders.GetAsync(orderId, cancellationToken);
        if (order is null ||
            order.Status != OrderStatus.PendingPayment ||
            order.CreatedAtUtc > timeout ||
            !order.TransitionTo(OrderStatus.Cancelled))
            return;

        try
        {
            await orders.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogInformation(
                "Skipped expiration for order {OrderId} because its status changed concurrently.",
                orderId);
            return;
        }

        await scope.ServiceProvider.GetRequiredService<IInventoryReservationService>()
            .ReleaseAsync(order.Id, cancellationToken);
        await scope.ServiceProvider.GetRequiredService<CosmicWiki.Ordering.Application.Abstractions.IOrderNotificationPublisher>()
            .PublishStatusChangedAsync(order, cancellationToken);
        await scope.ServiceProvider.GetRequiredService<IOrderEmailSender>()
            .SendOrderStatusChangedAsync(
                order.Id,
                order.CustomerEmail,
                order.TotalAmount,
                order.Currency,
                order.Status.ToString(),
                order.Lines.Select(line => new CosmicWiki.Ordering.Application.Events.OrderCreatedLine(
                    line.SKU,
                    line.ProductName,
                    line.UnitPrice,
                    line.Quantity,
                    line.LineTotal)).ToArray(),
                cancellationToken);
    }
}
