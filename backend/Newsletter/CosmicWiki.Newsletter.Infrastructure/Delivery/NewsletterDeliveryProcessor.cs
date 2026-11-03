using CosmicWiki.Newsletter.Application;
using CosmicWiki.Newsletter.Application.Features;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Newsletter.Infrastructure.Delivery;

public sealed class NewsletterDeliveryProcessor(
    IServiceScopeFactory scopeFactory,
    NewsletterOptions options,
    ILogger<NewsletterDeliveryProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Newsletter delivery processing failed.");
            }

            await Task.Delay(options.DeliveryPollInterval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
        await mediator.Send(new ProcessNewsletterDeliveriesCommand(), cancellationToken);
    }
}
