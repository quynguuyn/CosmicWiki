using CosmicWiki.Newsletter.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Newsletter.Application.Features;

public sealed record ProcessNewsletterDeliveriesCommand : IRequest;

public sealed class ProcessNewsletterDeliveriesHandler(
    INewsletterDeliveryRepository deliveries,
    INewsletterUnitOfWork unitOfWork,
    IEmailSender sender,
    NewsletterOptions options,
    TimeProvider timeProvider,
    ILogger<ProcessNewsletterDeliveriesHandler> logger)
    : IRequestHandler<ProcessNewsletterDeliveriesCommand>
{
    public async Task Handle(
        ProcessNewsletterDeliveriesCommand request,
        CancellationToken cancellationToken)
    {
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
                delivery.MarkFailed(
                    exception.Message,
                    timeProvider.GetUtcNow().UtcDateTime,
                    retryAt);
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
