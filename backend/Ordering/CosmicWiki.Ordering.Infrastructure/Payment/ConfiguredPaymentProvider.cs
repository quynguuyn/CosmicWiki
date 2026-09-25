using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Ordering.Infrastructure.Payment;

public sealed class ConfiguredPaymentProvider(
    IOptions<PaymentProviderOptions> options,
    FakePaymentProvider fakeProvider) : IPaymentProvider
{
    public Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        return options.Value.Mode.Trim().ToLowerInvariant() switch
        {
            "fake" => fakeProvider.CreatePaymentIntentAsync(
                orderId, amount, currency, idempotencyKey, cancellationToken),
            "stripe" => throw new InvalidOperationException(
                "Stripe provider was not registered."),
            _ => throw new InvalidOperationException(
                "No payment provider is configured. Set PaymentProvider:Mode to a supported provider before enabling checkout.")
        };
    }
}
