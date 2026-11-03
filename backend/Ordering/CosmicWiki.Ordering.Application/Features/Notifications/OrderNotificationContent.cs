using CosmicWiki.Ordering.Domain.Enums;

namespace CosmicWiki.Ordering.Application.Features.Notifications;

public static class OrderNotificationContent
{
    public static (string Title, string Message) For(Guid orderId, OrderStatus status) =>
        status switch
        {
            OrderStatus.PendingPayment => ("Đơn hàng đã tạo", $"Đơn hàng #{ShortId(orderId)} đang chờ thanh toán."),
            OrderStatus.Paid => ("Đã nhận thanh toán", $"Đơn hàng #{ShortId(orderId)} đã thanh toán và đang chờ xác nhận."),
            OrderStatus.Confirmed => ("Đơn hàng đã xác nhận", $"Đơn hàng #{ShortId(orderId)} đã được xác nhận và đang chờ nhân viên nhận giao."),
            OrderStatus.Shipping => ("Đơn hàng đang giao", $"Đơn hàng #{ShortId(orderId)} đang được giao đến bạn."),
            OrderStatus.Delivered => ("Đơn hàng đã giao", $"Đơn hàng #{ShortId(orderId)} đã được giao thành công."),
            OrderStatus.Returned => ("Đơn hàng đã trả", $"Đơn hàng #{ShortId(orderId)} đã được chuyển sang trạng thái trả hàng."),
            OrderStatus.Cancelled => ("Đơn hàng đã hủy", $"Đơn hàng #{ShortId(orderId)} đã bị hủy."),
            OrderStatus.PaymentFailed => ("Thanh toán thất bại", $"Thanh toán cho đơn hàng #{ShortId(orderId)} không thành công."),
            _ => throw new InvalidOperationException($"Notifications are not supported for order status '{status}'.")
        };

    private static string ShortId(Guid orderId) => orderId.ToString("N")[..8].ToUpperInvariant();
}
