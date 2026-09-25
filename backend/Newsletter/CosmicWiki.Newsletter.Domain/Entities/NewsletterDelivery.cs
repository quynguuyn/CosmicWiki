using CosmicWiki.Newsletter.Domain.Enums;

namespace CosmicWiki.Newsletter.Domain.Entities;

public sealed class NewsletterDelivery
{
    private NewsletterDelivery()
    {
    }

    public Guid Id { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public NewsletterSubscription Subscription { get; private set; } = null!;
    public DeliveryStatus Status { get; private set; }
    public string RecipientEmail { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public int Attempts { get; private set; }
    public DateTime? LastAttemptAtUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public string? LastError { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static NewsletterDelivery CreateConfirmation(
        NewsletterSubscription subscription,
        string body,
        DateTime nowUtc)
    {
        return new NewsletterDelivery
        {
            Id = Guid.NewGuid(),
            Subscription = subscription,
            SubscriptionId = subscription.Id,
            Status = DeliveryStatus.Pending,
            RecipientEmail = subscription.Email,
            Subject = "Confirm your CosmicWiki newsletter subscription",
            Body = body,
            NextAttemptAtUtc = nowUtc,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    public void BeginAttempt(DateTime nowUtc)
    {
        Status = DeliveryStatus.Processing;
        Attempts++;
        LastAttemptAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        LastError = null;
    }

    public void MarkSent(DateTime nowUtc)
    {
        Status = DeliveryStatus.Sent;
        SentAtUtc = nowUtc;
        UpdatedAtUtc = nowUtc;
        NextAttemptAtUtc = null;
        LastError = null;
    }

    public void MarkFailed(string error, DateTime nowUtc, DateTime? nextAttemptAtUtc)
    {
        Status = DeliveryStatus.Failed;
        LastError = error[..Math.Min(error.Length, 2000)];
        NextAttemptAtUtc = nextAttemptAtUtc;
        UpdatedAtUtc = nowUtc;
    }
}
