using CosmicWiki.Ordering.Application.Abstractions;

namespace CosmicWiki.Ordering.Infrastructure.Processing;

public sealed class OrderLifecycleOptions : IOrderLifecycleSettings
{
    public const string SectionName = "OrderLifecycle";
    public int PendingPaymentTimeoutMinutes { get; set; } = 15;
}
