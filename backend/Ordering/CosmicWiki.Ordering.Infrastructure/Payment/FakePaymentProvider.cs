using System.Collections.Concurrent;
using CosmicWiki.Ordering.Application.Abstractions;

namespace CosmicWiki.Ordering.Infrastructure.Payment;

public sealed class FakePaymentProvider : IPaymentProvider
{
    private readonly ConcurrentDictionary<string, PaymentIntentResult> _intents = new();

    public Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId, decimal amount, string currency, string idempotencyKey, CancellationToken cancellationToken)
    {
        var result = _intents.GetOrAdd(
            idempotencyKey,
            _ => new PaymentIntentResult($"fake_pi_{orderId:N}", "requires_payment_method"));
        return Task.FromResult(result);
    }
}
