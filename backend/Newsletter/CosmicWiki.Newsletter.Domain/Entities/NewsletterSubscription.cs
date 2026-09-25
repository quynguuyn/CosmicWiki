using CosmicWiki.Newsletter.Domain.Enums;

namespace CosmicWiki.Newsletter.Domain.Entities;

public sealed class NewsletterSubscription
{
    private NewsletterSubscription()
    {
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; }
    public string ConfirmationTokenHash { get; private set; } = null!;
    public DateTime ConfirmationTokenExpiresAtUtc { get; private set; }
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? UnsubscribedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public ICollection<NewsletterDelivery> Deliveries { get; private set; } = new List<NewsletterDelivery>();

    public static NewsletterSubscription Create(
        string email,
        string confirmationTokenHash,
        DateTime tokenExpiresAtUtc,
        DateTime nowUtc)
    {
        return new NewsletterSubscription
        {
            Id = Guid.NewGuid(),
            Email = NormalizeEmail(email),
            Status = SubscriptionStatus.PendingConfirmation,
            ConfirmationTokenHash = confirmationTokenHash,
            ConfirmationTokenExpiresAtUtc = tokenExpiresAtUtc,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    public bool IsConfirmationTokenExpired(DateTime nowUtc) =>
        ConfirmationTokenExpiresAtUtc <= nowUtc;

    public void Confirm(DateTime nowUtc)
    {
        if (Status == SubscriptionStatus.Unsubscribed)
        {
            return;
        }

        Status = SubscriptionStatus.Confirmed;
        ConfirmedAtUtc ??= nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void Unsubscribe(DateTime nowUtc)
    {
        Status = SubscriptionStatus.Unsubscribed;
        UnsubscribedAtUtc ??= nowUtc;
        UpdatedAtUtc = nowUtc;
    }

    public void RenewConfirmation(
        string confirmationTokenHash,
        DateTime tokenExpiresAtUtc,
        DateTime nowUtc)
    {
        Status = SubscriptionStatus.PendingConfirmation;
        ConfirmationTokenHash = confirmationTokenHash;
        ConfirmationTokenExpiresAtUtc = tokenExpiresAtUtc;
        ConfirmedAtUtc = null;
        UnsubscribedAtUtc = null;
        UpdatedAtUtc = nowUtc;
    }

    public static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
