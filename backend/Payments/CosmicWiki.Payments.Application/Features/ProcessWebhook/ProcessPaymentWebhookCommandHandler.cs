using CosmicWiki.Payments.Application.Abstractions;
using CosmicWiki.Payments.Domain.Entities;
using MediatR;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CosmicWiki.Payments.Application.Features.ProcessWebhook;

public sealed class ProcessPaymentWebhookCommandHandler :
    IRequestHandler<ProcessPaymentWebhookCommand, PaymentWebhookProcessingResult>
{
    private readonly IPaymentRepository _payments;
    private readonly IOrderPaymentValidator _orders;
    private readonly IPaymentEventWriter _events;

    private static readonly Meter Meter = new("CosmicWiki.Payments.Webhook");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("webhook.duration", "ms");
    private static readonly Counter<long> Processed = Meter.CreateCounter<long>("webhook.processed");
    private static readonly Counter<long> Failed = Meter.CreateCounter<long>("webhook.failed");

    public ProcessPaymentWebhookCommandHandler(
        IPaymentRepository payments,
        IOrderPaymentValidator orders,
        IPaymentEventWriter events)
    {
        _payments = payments;
        _orders = orders;
        _events = events;
    }

    public async Task<PaymentWebhookProcessingResult> Handle(
        ProcessPaymentWebhookCommand request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await HandleInternal(request, cancellationToken);
            if (result == PaymentWebhookProcessingResult.Processed || result == PaymentWebhookProcessingResult.Duplicate)
                Processed.Add(1);
            else
                Failed.Add(1);
            return result;
        }
        catch
        {
            Failed.Add(1);
            throw;
        }
        finally
        {
            sw.Stop();
            Duration.Record(sw.Elapsed.TotalMilliseconds);
        }
    }

    private async Task<PaymentWebhookProcessingResult> HandleInternal(
        ProcessPaymentWebhookCommand request,
        CancellationToken cancellationToken)
    {
        if (!IsValidRequest(request))
            return PaymentWebhookProcessingResult.Invalid;

        if (!await _orders.IsPaymentIntentValidAsync(
                request.OrderId,
                request.PaymentIntentId,
                request.Amount,
                request.Currency,
                cancellationToken))
            return PaymentWebhookProcessingResult.Invalid;

        if (await _payments.HasProcessedEventAsync(request.ProviderEventId, cancellationToken))
            return PaymentWebhookProcessingResult.Duplicate;

        var payment = await _payments.GetByIntentIdAsync(request.PaymentIntentId, cancellationToken);
        if (payment is null)
        {
            payment = Payment.Create(request.OrderId, request.PaymentIntentId, request.Amount, request.Currency);
            _payments.Add(payment);
        }

        var succeeded = string.Equals(request.Status, "succeeded", StringComparison.OrdinalIgnoreCase);
        if (succeeded)
        {
            payment.MarkSucceeded();
        }
        else if (string.Equals(request.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            payment.MarkFailed();
        }
        else
        {
            return PaymentWebhookProcessingResult.Invalid;
        }

        _payments.AddProcessedEvent(request.ProviderEventId);
        _events.AddStatusChanged(payment);
        return await _payments.TrySaveChangesAsync(
                request.ProviderEventId,
                cancellationToken)
            ? PaymentWebhookProcessingResult.Processed
            : PaymentWebhookProcessingResult.Duplicate;
    }

    private static bool IsValidRequest(ProcessPaymentWebhookCommand request)
    {
        return request.ProviderEventId is { Length: > 0 and <= 200 } &&
               !request.ProviderEventId.Any(char.IsControl) &&
               request.OrderId != Guid.Empty &&
               request.PaymentIntentId is { Length: > 0 and <= 200 } &&
               !request.PaymentIntentId.Any(char.IsControl) &&
               request.Amount >= 0 &&
               request.Currency is { Length: 3 } &&
               request.Currency.All(char.IsAsciiLetter) &&
               (string.Equals(request.Status, "succeeded", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.Status, "failed", StringComparison.OrdinalIgnoreCase));
    }
}
