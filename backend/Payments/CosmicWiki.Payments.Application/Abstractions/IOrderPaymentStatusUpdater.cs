namespace CosmicWiki.Payments.Application.Abstractions;

public interface IOrderPaymentStatusUpdater
{
    Task<bool> IsPaymentIntentValidAsync(Guid orderId, string paymentIntentId, decimal amount, string currency, CancellationToken cancellationToken);
    Task<bool> MarkPaidAsync(Guid orderId, CancellationToken cancellationToken);
    Task MarkPaymentFailedAsync(Guid orderId, CancellationToken cancellationToken);
}
