using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Ordering.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];
    private Order() : base(Guid.Empty) { }

    private Order(Guid id, Guid customerId, string currency, string idempotencyKey, string requestFingerprint) : base(id)
    {
        CustomerId = customerId;
        Currency = currency;
        IdempotencyKey = idempotencyKey;
        RequestFingerprint = requestFingerprint;
        Status = OrderStatus.PendingPayment;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public string Currency { get; private set; } = null!;
    public string IdempotencyKey { get; private set; } = null!;
    public string RequestFingerprint { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? PaymentIntentId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public static Order Create(
        Guid customerId,
        string currency,
        string idempotencyKey,
        string requestFingerprint,
        IEnumerable<OrderLine> lines)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestFingerprint);
        if (currency.Length != 3 || currency.Any(c => !char.IsAsciiLetter(c)))
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(currency));
        if (currency.Any(char.IsControl))
            throw new ArgumentException("Currency contains invalid characters.", nameof(currency));
        if (idempotencyKey.Length > 200 || idempotencyKey.Any(char.IsControl) || idempotencyKey != idempotencyKey.Trim())
            throw new ArgumentException("Idempotency key must be 1-200 characters without surrounding whitespace or control characters.", nameof(idempotencyKey));
        var order = new Order(Guid.NewGuid(), customerId, currency.Trim().ToUpperInvariant(),
            idempotencyKey.Trim(), requestFingerprint.Trim());
        order._lines.AddRange(lines ?? throw new ArgumentNullException(nameof(lines)));
        if (order._lines.Count is < 1 or > 100) throw new ArgumentException("Order must contain between 1 and 100 lines.", nameof(lines));
        if (order._lines.Select(x => x.ProductId).Distinct().Count() != order._lines.Count)
            throw new ArgumentException("An order cannot contain duplicate products.", nameof(lines));
        order.TotalAmount = order._lines.Sum(x => x.LineTotal);
        return order;
    }

    public void AttachPaymentIntent(string paymentIntentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentIntentId);
        PaymentIntentId = paymentIntentId.Trim();
    }

    public void MarkPaid()
    {
        if (Status is OrderStatus.Cancelled) return;
        Status = OrderStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
    }

    public void MarkPaymentFailed()
    {
        if (Status is not OrderStatus.Paid and not OrderStatus.Cancelled)
            Status = OrderStatus.PaymentFailed;
    }
}
