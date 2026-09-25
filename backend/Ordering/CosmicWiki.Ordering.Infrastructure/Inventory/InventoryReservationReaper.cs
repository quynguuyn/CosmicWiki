using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CosmicWiki.Ordering.Application.Abstractions;

namespace CosmicWiki.Ordering.Infrastructure.Inventory;

public sealed class InventoryReservationReaper(
    IServiceScopeFactory scopeFactory,
    ILogger<InventoryReservationReaper> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var reservations = scope.ServiceProvider.GetRequiredService<IInventoryReservationService>();
                await reservations.ReleaseExpiredAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to release expired inventory reservations.");
            }
        }
    }
}
