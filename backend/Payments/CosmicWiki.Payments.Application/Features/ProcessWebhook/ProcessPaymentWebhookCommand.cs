using MediatR;

namespace CosmicWiki.Payments.Application.Features.ProcessWebhook;

public enum PaymentWebhookProcessingResult
{
    Processed,
    Duplicate,
    Invalid
}

public sealed record ProcessPaymentWebhookCommand(
    string ProviderEventId,
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    string Status) : IRequest<PaymentWebhookProcessingResult>;
