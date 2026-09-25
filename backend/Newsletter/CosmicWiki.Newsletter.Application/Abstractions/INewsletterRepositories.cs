using CosmicWiki.Newsletter.Domain.Entities;

namespace CosmicWiki.Newsletter.Application.Abstractions;

public interface INewsletterSubscriptionRepository
{
    Task AddAsync(NewsletterSubscription subscription, CancellationToken cancellationToken);
    Task<NewsletterSubscription?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<NewsletterSubscription?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
}

public interface INewsletterDeliveryRepository
{
    Task AddAsync(NewsletterDelivery delivery, CancellationToken cancellationToken);
    Task<IReadOnlyList<NewsletterDelivery>> GetDueAsync(
        DateTime nowUtc,
        int limit,
        CancellationToken cancellationToken);
}

public interface INewsletterUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record EmailMessage(
    string RecipientEmail,
    string Subject,
    string Body);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public interface IConfirmationTokenService
{
    (string RawToken, string Hash) Create();
    string Hash(string rawToken);
}
