using CosmicWiki.Ordering.Domain.Enums;
using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Ordering.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];
    private readonly List<OrderStatusHistoryEntry> _statusHistory = [];
    private Order() : base(Guid.Empty) { }

    private Order(
        Guid id,
        Guid customerId,
        string customerEmail,
        string currency,
        string idempotencyKey,
        string requestFingerprint,
        string? deliveryAddress) : base(id)
    {
        CustomerId = customerId;
        CustomerEmail = customerEmail;
        Currency = currency;
        IdempotencyKey = idempotencyKey;
        RequestFingerprint = requestFingerprint;
        DeliveryAddress = deliveryAddress;
        Status = OrderStatus.PendingPayment;
        CreatedAtUtc = DateTime.UtcNow;
        RecordStatusChange();
    }

    public Guid CustomerId { get; private set; }
    public string? CustomerEmail { get; private set; }
    public string Currency { get; private set; } = null!;
    public string IdempotencyKey { get; private set; } = null!;
    public string RequestFingerprint { get; private set; } = null!;
    public string? DeliveryAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? PaymentIntentId { get; private set; }
    public string? ClientSecret { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public Guid? DeliveryPersonId { get; private set; }
    public string? DeliveryPersonName { get; private set; }
    public double? DeliveryLatitude { get; private set; }
    public double? DeliveryLongitude { get; private set; }
    public DateTime? DeliveryLocationUpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<OrderStatusHistoryEntry> StatusHistory => _statusHistory.AsReadOnly();

    public static Order Create(
        Guid customerId,
        string customerEmail,
        string currency,
        string idempotencyKey,
        string requestFingerprint,
        IEnumerable<OrderLine> lines,
        string? deliveryAddress = null)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestFingerprint);
        if (currency.Length != 3 || currency.Any(c => !char.IsAsciiLetter(c)))
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(currency));
        if (currency.Any(char.IsControl))
            throw new ArgumentException("Currency contains invalid characters.", nameof(currency));
        if (idempotencyKey.Length > 200 || idempotencyKey.Any(char.IsControl) || idempotencyKey != idempotencyKey.Trim())
            throw new ArgumentException("Idempotency key must be 1-200 characters without surrounding whitespace or control characters.", nameof(idempotencyKey));
        if (deliveryAddress?.Length > 500)
            throw new ArgumentException("Delivery address cannot exceed 500 characters.", nameof(deliveryAddress));
        var order = new Order(
            Guid.NewGuid(),
            customerId,
            customerEmail.Trim(),
            currency.Trim().ToUpperInvariant(),
            idempotencyKey.Trim(),
            requestFingerprint.Trim(),
            string.IsNullOrWhiteSpace(deliveryAddress) ? null : deliveryAddress.Trim());
        order._lines.AddRange(lines ?? throw new ArgumentNullException(nameof(lines)));
        if (order._lines.Count is < 1 or > 100) throw new ArgumentException("Order must contain between 1 and 100 lines.", nameof(lines));
        if (order._lines.Select(x => x.ProductId).Distinct().Count() != order._lines.Count)
            throw new ArgumentException("An order cannot contain duplicate products.", nameof(lines));
        order.TotalAmount = order._lines.Sum(x => x.LineTotal);
        return order;
    }

    public void AttachPaymentIntent(string paymentIntentId, string? clientSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentIntentId);
        PaymentIntentId = paymentIntentId.Trim();
        ClientSecret = clientSecret?.Trim();
    }

    public void MarkPaid()
    {
        if (Status is OrderStatus.Cancelled) return;
        if (Status is OrderStatus.Paid or OrderStatus.Confirmed or OrderStatus.Shipping) return;
        Status = OrderStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
        RecordStatusChange();
    }

    public void MarkPaymentFailed()
    {
        if (Status is not OrderStatus.Paid and not OrderStatus.Confirmed
            and not OrderStatus.Shipping and not OrderStatus.Cancelled)
        {
            if (Status == OrderStatus.PaymentFailed) return;
            Status = OrderStatus.PaymentFailed;
            RecordStatusChange();
        }
    }

    public bool TransitionTo(OrderStatus nextStatus)
    {
        if (Status == nextStatus)
            return false;

        var valid = (Status, nextStatus) switch
        {
            (OrderStatus.Paid, OrderStatus.Confirmed) => true,
            (OrderStatus.Confirmed, OrderStatus.Shipping) => true,
            (OrderStatus.Shipping, OrderStatus.Delivered) => true,
            (OrderStatus.Delivered, OrderStatus.Returned) => true,
            (OrderStatus.PendingPayment, OrderStatus.Cancelled) => true,
            (OrderStatus.PaymentFailed, OrderStatus.Cancelled) => true,
            (OrderStatus.Paid, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipping, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException(
                $"Order cannot transition from '{Status}' to '{nextStatus}'.");

        Status = nextStatus;
        RecordStatusChange();
        return true;
    }

    public void AssignDeliveryPerson(Guid deliveryPersonId, string deliveryPersonName)
    {
        if (deliveryPersonId == Guid.Empty)
            throw new ArgumentException("Delivery person ID is required.", nameof(deliveryPersonId));
        if (Status != OrderStatus.Confirmed || DeliveryPersonId is not null)
            throw new InvalidOperationException("Only an unassigned confirmed order can be claimed for delivery.");
        ArgumentException.ThrowIfNullOrWhiteSpace(deliveryPersonName);

        DeliveryPersonId = deliveryPersonId;
        DeliveryPersonName = deliveryPersonName.Trim();
        TransitionTo(OrderStatus.Shipping);
    }

    public void UpdateDeliveryLocation(Guid deliveryPersonId, double latitude, double longitude)
    {
        EnsureAssignedDeliveryPerson(deliveryPersonId);
        if (Status != OrderStatus.Shipping)
            throw new InvalidOperationException("Location can only be updated while the order is shipping.");
        if (!double.IsFinite(latitude) || latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (!double.IsFinite(longitude) || longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));

        DeliveryLatitude = latitude;
        DeliveryLongitude = longitude;
        DeliveryLocationUpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkDeliveredBy(Guid deliveryPersonId)
    {
        EnsureAssignedDeliveryPerson(deliveryPersonId);
        TransitionTo(OrderStatus.Delivered);
    }

    private void EnsureAssignedDeliveryPerson(Guid deliveryPersonId)
    {
        if (deliveryPersonId == Guid.Empty || DeliveryPersonId != deliveryPersonId)
            throw new InvalidOperationException("This delivery is not assigned to the current account.");
    }

    private void RecordStatusChange() =>
        _statusHistory.Add(
            OrderStatusHistoryEntry.Create(Id, Status, DateTime.UtcNow));
}
