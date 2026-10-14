namespace CosmicWiki.Ordering.Domain.Enums;

public enum OrderStatus
{
    PendingPayment = 1,
    Paid = 2,
    PaymentFailed = 3,
    Cancelled = 4,
    Delivered = 5,
    Returned = 6,
    Shipping = 7
}
