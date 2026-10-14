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
                var inventory = scope.ServiceProvider.GetRequiredService<IInventoryReservationService>();
                var email = scope.ServiceProvider.GetRequiredService<IOrderEmailSender>();
                var now = DateTime.UtcNow;
                var timeout = now.AddMinutes(-Math.Max(1, options.Value.PendingPaymentTimeoutMinutes));
                var expired = await db.Orders
                    .Where(x => x.Status == OrderStatus.PendingPayment && x.CreatedAtUtc <= timeout)
                    .ToListAsync(stoppingToken);

                foreach (var order in expired)
                {
                    order.TransitionTo(OrderStatus.Cancelled);
                    await inventory.ReleaseAsync(order.Id, stoppingToken);
                }

                var cutoff = new DateTime(
                    now.Year, now.Month, now.Day,
                    Math.Clamp(options.Value.AutoConfirmAtUtcHour, 0, 23), 0, 0,
                    DateTimeKind.Utc);
                var toShip = new List<Domain.Entities.Order>();
                if (now >= cutoff)
                {
                    toShip.AddRange(await db.Orders
                        .Where(x => x.Status == OrderStatus.Paid && x.CreatedAtUtc < cutoff)
                        .ToListAsync(stoppingToken));
                    foreach (var order in toShip)
                        order.TransitionTo(OrderStatus.Shipping);
                }

                var changed = expired.Count > 0 || toShip.Count > 0;
                if (!changed)
                    continue;

                await db.SaveChangesAsync(stoppingToken);
                foreach (var order in expired.Concat(toShip))
                    await email.SendOrderStatusChangedAsync(
                        order.Id, order.CustomerEmail, order.TotalAmount, order.Currency,
                        order.Status.ToString(), stoppingToken);
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
}
