using CosmicWiki.Newsletter.Application;
using CosmicWiki.Newsletter.Application.Abstractions;
using CosmicWiki.Newsletter.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        var deliveries = scope.ServiceProvider.GetRequiredService<INewsletterDeliveryRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<INewsletterUnitOfWork>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var due = await deliveries.GetDueAsync(
            now,
            options.DeliveryBatchSize,
            cancellationToken);

        foreach (var delivery in due)
        {
            if (delivery.Attempts >= options.MaxDeliveryAttempts)
            {
                continue;
            }

            delivery.BeginAttempt(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await sender.SendAsync(
                    new EmailMessage(
                        delivery.RecipientEmail,
                        delivery.Subject,
                        delivery.Body),
                    cancellationToken);
                delivery.MarkSent(timeProvider.GetUtcNow().UtcDateTime);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                var attempt = delivery.Attempts;
                DateTime? retryAt = attempt >= options.MaxDeliveryAttempts
                    ? null
                    : timeProvider.GetUtcNow().UtcDateTime.Add(
                        TimeSpan.FromMinutes(Math.Min(Math.Pow(2, attempt), 60)));
                delivery.MarkFailed(exception.Message, timeProvider.GetUtcNow().UtcDateTime, retryAt);
                logger.LogError(
                    exception,
                    "Newsletter delivery {DeliveryId} failed on attempt {Attempt}.",
                    delivery.Id,
                    attempt);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
