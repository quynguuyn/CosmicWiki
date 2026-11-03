namespace CosmicWiki.Ordering.Application.Abstractions;

public interface IOrderLifecycleSettings
{
    int PendingPaymentTimeoutMinutes { get; }
}
