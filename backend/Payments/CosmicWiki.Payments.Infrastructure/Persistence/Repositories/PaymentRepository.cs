using CosmicWiki.Payments.Application.Abstractions;
using CosmicWiki.Payments.Domain.Entities;
using CosmicWiki.Payments.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Payments.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
{
    public async Task<IReadOnlyList<PaymentReconciliationRecord>> GetSettledAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (batchSize is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(batchSize));

        return await dbContext.Payments
            .AsNoTracking()
            .Where(x => x.Status != PaymentStatus.Pending && x.ReconciledAtUtc == null)
            .OrderBy(x => x.CompletedAtUtc)
            .Take(batchSize)
            .Select(x => new PaymentReconciliationRecord(
                x.OrderId,
                x.PaymentIntentId,
                x.Amount,
                x.Currency,
                x.Status == PaymentStatus.Succeeded))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkReconciledAsync(
        string paymentIntentId,
        CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .SingleOrDefaultAsync(
                x => x.PaymentIntentId == paymentIntentId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Payment '{paymentIntentId}' was not found.");

        payment.MarkReconciled();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasProcessedEventAsync(string providerEventId, CancellationToken cancellationToken) =>
        dbContext.ProcessedProviderEvents.AnyAsync(x => x.ProviderEventId == providerEventId, cancellationToken);

    public Task<Payment?> GetByIntentIdAsync(string intentId, CancellationToken cancellationToken) =>
        dbContext.Payments.SingleOrDefaultAsync(x => x.PaymentIntentId == intentId, cancellationToken);

    public void Add(Payment payment) => dbContext.Payments.Add(payment);
    public void AddProcessedEvent(string providerEventId) => dbContext.ProcessedProviderEvents.Add(new ProcessedProviderEvent(providerEventId));
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> TrySaveChangesAsync(
        string providerEventId,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            dbContext.ChangeTracker.Clear();
            if (await HasProcessedEventAsync(providerEventId, cancellationToken))
                return false;

            throw;
        }
    }
}
