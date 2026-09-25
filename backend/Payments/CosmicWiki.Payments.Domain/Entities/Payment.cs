using CosmicWiki.Payments.Domain.Enums;

namespace CosmicWiki.Payments.Domain.Entities;

public sealed class Payment
{
    private Payment() { }

    private Payment(Guid id, Guid orderId, string intentId, decimal amount, string currency)
    {
        Id = id;
        OrderId = orderId;
        PaymentIntentId = intentId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string PaymentIntentId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? ReconciledAtUtc { get; private set; }

    public static Payment Create(Guid orderId, string intentId, decimal amount, string currency)
    {
        if (orderId == Guid.Empty) throw new ArgumentException("Order ID is required.", nameof(orderId));
        ArgumentException.ThrowIfNullOrWhiteSpace(intentId);
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        return new Payment(Guid.NewGuid(), orderId, intentId.Trim(), amount, currency.Trim().ToUpperInvariant());
    }

    public void MarkSucceeded()
    {
        Status = PaymentStatus.Succeeded;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        if (Status != PaymentStatus.Succeeded)
            Status = PaymentStatus.Failed;
    }

    public void MarkReconciled()
    {
        ReconciledAtUtc = DateTime.UtcNow;
    }
}
