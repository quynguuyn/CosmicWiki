namespace CosmicWiki.Ordering.Application.Abstractions;

public sealed record PaymentIntentResult(string PaymentIntentId, string Status);

public interface IPaymentProvider
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId, decimal amount, string currency, string idempotencyKey, CancellationToken cancellationToken);
}
