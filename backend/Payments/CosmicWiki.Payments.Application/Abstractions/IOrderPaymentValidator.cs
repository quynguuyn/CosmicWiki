namespace CosmicWiki.Payments.Application.Abstractions;

public interface IOrderPaymentValidator
{
    Task<bool> IsPaymentIntentValidAsync(
        Guid orderId,
        string paymentIntentId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken);
}
