using CosmicWiki.Payments.Domain.Entities;

namespace CosmicWiki.Payments.Application.Abstractions;

public interface IPaymentRepository
{
    Task<IReadOnlyList<PaymentReconciliationRecord>> GetSettledAsync(
        int batchSize,
        CancellationToken cancellationToken);
    Task MarkReconciledAsync(
        string paymentIntentId,
        CancellationToken cancellationToken);
    Task<bool> HasProcessedEventAsync(string providerEventId, CancellationToken cancellationToken);
    Task<Payment?> GetByIntentIdAsync(string intentId, CancellationToken cancellationToken);
    void Add(Payment payment);
    void AddProcessedEvent(string providerEventId);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<bool> TrySaveChangesAsync(string providerEventId, CancellationToken cancellationToken);
}

public sealed record PaymentReconciliationRecord(
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    bool Succeeded);
