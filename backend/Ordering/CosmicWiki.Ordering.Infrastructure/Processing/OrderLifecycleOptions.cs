namespace CosmicWiki.Ordering.Infrastructure.Processing;

public sealed class OrderLifecycleOptions
{
    public const string SectionName = "OrderLifecycle";
    public int PendingPaymentTimeoutMinutes { get; set; } = 15;
}
