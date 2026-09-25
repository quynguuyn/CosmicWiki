using CosmicWiki.Newsletter.Application.Abstractions;
using CosmicWiki.Newsletter.Domain.Entities;
using CosmicWiki.Newsletter.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Newsletter.Infrastructure.Persistence;

public sealed class NewsletterSubscriptionRepository(NewsletterDbContext db)
    : INewsletterSubscriptionRepository
{
    public Task AddAsync(
        NewsletterSubscription subscription,
        CancellationToken cancellationToken) =>
        db.Subscriptions.AddAsync(subscription, cancellationToken).AsTask();

    public Task<NewsletterSubscription?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken) =>
        db.Subscriptions.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<NewsletterSubscription?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken) =>
        db.Subscriptions.SingleOrDefaultAsync(
            x => x.ConfirmationTokenHash == tokenHash,
            cancellationToken);
}

public sealed class NewsletterDeliveryRepository(NewsletterDbContext db)
    : INewsletterDeliveryRepository
{
    public Task AddAsync(
        NewsletterDelivery delivery,
        CancellationToken cancellationToken) =>
        db.Deliveries.AddAsync(delivery, cancellationToken).AsTask();

    public async Task<IReadOnlyList<NewsletterDelivery>> GetDueAsync(
        DateTime nowUtc,
        int limit,
        CancellationToken cancellationToken)
    {
        return await db.Deliveries
            .Where(x =>
                (x.Status == DeliveryStatus.Pending ||
                 x.Status == DeliveryStatus.Failed) &&
                x.NextAttemptAtUtc <= nowUtc)
            .OrderBy(x => x.NextAttemptAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
