namespace CosmicWiki.Ordering.Application.Abstractions;

public interface IOrderPaymentIntentValidator
{
    Task<bool> IsPaymentIntentValidAsync(
        Guid orderId,
        string paymentIntentId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken);
}
