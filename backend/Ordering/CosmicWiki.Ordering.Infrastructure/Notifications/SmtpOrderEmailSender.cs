using System.Net;
using System.Net.Mail;
using System.Text;
using CosmicWiki.Ordering.Application.Events;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Ordering.Infrastructure.Notifications;

public sealed class SmtpOrderEmailSender(
    IOptions<OrderEmailOptions> options,
    ILogger<SmtpOrderEmailSender> logger) : IOrderEmailSender
{
    private readonly OrderEmailOptions _options = options.Value;

    public async Task SendOrderCreatedAsync(
        OrderCreatedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.CustomerEmail))
        {
            logger.LogWarning(
                "Order {OrderId} has no customer email. Notification skipped.",
                notification.AggregateId);
            return;
        }

        if (!_options.Enabled)
        {
            logger.LogInformation(
                "Order email is disabled. Skipping notification for order {OrderId}.",
                notification.AggregateId);
            return;
        }

        ValidateConfiguration();

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = $"CosmicWiki đã tiếp nhận đơn hàng #{notification.AggregateId.ToString()[..8]}",
            Body = BuildHtml(notification),
            IsBodyHtml = true,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(notification.CustomerEmail));

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        await client.SendMailAsync(message, cancellationToken);
        logger.LogInformation(
            "Order confirmation email sent for order {OrderId}.",
            notification.AggregateId);
    }

    public async Task SendOrderStatusChangedAsync(
        Guid orderId,
        string? customerEmail,
        decimal totalAmount,
        string currency,
        string status,
        IReadOnlyCollection<OrderCreatedLine> lines,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
            return;
        if (!_options.Enabled)
        {
            logger.LogInformation("Order email is disabled. Skipping status notification for order {OrderId}.", orderId);
            return;
        }

        ValidateConfiguration();
        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = $"CosmicWiki cập nhật trạng thái đơn hàng #{orderId.ToString()[..8]}",
            Body = BuildStatusHtml(orderId, totalAmount, currency, status, lines),
            IsBodyHtml = true,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(customerEmail));

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        await client.SendMailAsync(message, cancellationToken);
        logger.LogInformation("Order status email sent for order {OrderId} with status {Status}.", orderId, status);
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException(
                "Order email is enabled but SMTP host and FromAddress are not configured.");
    }

    private static string BuildHtml(OrderCreatedIntegrationEvent notification)
    {
        var orderId = notification.AggregateId.ToString();
        var shortId = orderId[..8].ToUpperInvariant();
        var amount = notification.TotalAmount.ToString("N2");
        var currency = notification.Currency.ToUpperInvariant();
        var lineItems = BuildLineItemsHtml(notification.Lines ?? [], currency);

        return $$"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#07111f;color:#d9e3f7;font-family:Arial,sans-serif;">
              <div style="padding:32px 16px;background:radial-gradient(circle at 50% 0,#1d3850 0,#07111f 58%);">
                <div style="max-width:620px;margin:0 auto;border:1px solid #38556b;border-radius:18px;background:#0c182b;overflow:hidden;">
                  <div style="padding:28px 32px;border-bottom:1px solid #294154;">
                    <div style="color:#e5c281;font-size:12px;letter-spacing:2px;">COSMICWIKI · ORDERING API</div>
                    <h1 style="margin:14px 0 0;color:#f3e9d2;font-size:28px;">Đơn hàng đã được tiếp nhận</h1>
                  </div>
                  <div style="padding:28px 32px;">
                    <p style="color:#b8d4e8;line-height:1.7;">Cảm ơn bạn đã lựa chọn lưu trữ hiện vật cùng CosmicWiki. Hệ thống đã tạo đơn hàng và đang chờ bạn hoàn tất thanh toán.</p>
                    <div style="margin:24px 0;padding:18px;border:1px solid #38556b;border-radius:12px;background:#101f33;">
                      <div style="color:#8eb9d6;font-size:12px;">MÃ ĐƠN HÀNG</div>
                      <div style="margin-top:6px;color:#f3e9d2;font-size:20px;font-weight:bold;">#{{shortId}}</div>
                      <div style="margin-top:16px;color:#8eb9d6;font-size:12px;">TỔNG GIÁ TRỊ</div>
                      <div style="margin-top:6px;color:#e5c281;font-size:20px;font-weight:bold;">{{amount}} {{currency}}</div>
                    </div>
                      {{lineItems}}
                      <p style="color:#b8d4e8;line-height:1.7;">Trạng thái hiện tại: <strong style="color:#e5c281;">Chờ thanh toán</strong>. Bạn có thể tiếp tục thanh toán từ trang đơn hàng trong ứng dụng.</p>
                  </div>
                  <div style="padding:20px 32px;border-top:1px solid #294154;color:#7898ae;font-size:12px;line-height:1.6;">Email này được gửi tự động từ CosmicWiki. Vui lòng không trả lời trực tiếp.</div>
                </div>
              </div>
            </body>
            </html>
            """;
    }

    private static string BuildStatusHtml(
        Guid orderId,
        decimal totalAmount,
        string currency,
        string status,
        IReadOnlyCollection<OrderCreatedLine> lines)
    {
        var label = status switch
        {
            "Paid" => "Đã thanh toán, chờ xác nhận",
            "Confirmed" => "Đơn hàng đã được xác nhận",
            "PaymentFailed" => "Thanh toán thất bại",
            "Shipping" => "Đang giao hàng",
            "Delivered" => "Đã giao hàng",
            "Returned" => "Đã trả hàng",
            "Cancelled" => "Đã hủy",
            _ => status
        };
        var lineItems = BuildLineItemsHtml(lines, currency);
        return $$"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#07111f;color:#d9e3f7;font-family:Arial,sans-serif;">
              <div style="padding:32px 16px;background:radial-gradient(circle at 50% 0,#1d3850 0,#07111f 58%);">
                <div style="max-width:620px;margin:0 auto;border:1px solid #38556b;border-radius:18px;background:#0c182b;overflow:hidden;">
                  <div style="padding:28px 32px;border-bottom:1px solid #294154;">
                    <div style="color:#e5c281;font-size:12px;letter-spacing:2px;">COSMICWIKI · ORDERING API</div>
                    <h1 style="margin:14px 0 0;color:#f3e9d2;font-size:28px;">Cập nhật đơn hàng</h1>
                  </div>
                  <div style="padding:28px 32px;">
                    <p style="color:#b8d4e8;line-height:1.7;">Đơn hàng <strong style="color:#f3e9d2;">#{{orderId.ToString()[..8].ToUpperInvariant()}}</strong> vừa được cập nhật.</p>
                    <div style="margin:24px 0;padding:18px;border:1px solid #38556b;border-radius:12px;background:#101f33;">
                      <div style="color:#8eb9d6;font-size:12px;">TRẠNG THÁI</div>
                      <div style="margin-top:6px;color:#e5c281;font-size:20px;font-weight:bold;">{{label}}</div>
                      <div style="margin-top:16px;color:#8eb9d6;font-size:12px;">TỔNG GIÁ TRỊ</div>
                      <div style="margin-top:6px;color:#e5c281;font-size:20px;font-weight:bold;">{{totalAmount:N2}} {{currency.ToUpperInvariant()}}</div>
                    </div>
                    {{lineItems}}
                  </div>
                  <div style="padding:20px 32px;border-top:1px solid #294154;color:#7898ae;font-size:12px;">Email tự động từ CosmicWiki.</div>
                </div>
              </div>
            </body>
            </html>
            """;
    }

    private static string BuildLineItemsHtml(IEnumerable<OrderCreatedLine> lines, string currency)
    {
        var rows = string.Join(
            string.Empty,
            lines.Select(line => $$"""
                <tr>
                  <td style="padding:10px 8px;border-bottom:1px solid #294154;color:#d9e3f7;">
                    <strong>{{WebUtility.HtmlEncode(line.ProductName)}}</strong>
                    <div style="margin-top:4px;color:#7898ae;font-size:12px;">SKU: {{WebUtility.HtmlEncode(line.SKU)}}</div>
                  </td>
                  <td style="padding:10px 8px;border-bottom:1px solid #294154;text-align:center;color:#d9e3f7;">{{line.Quantity}}</td>
                  <td style="padding:10px 8px;border-bottom:1px solid #294154;text-align:right;color:#d9e3f7;">{{line.UnitPrice:N2}} {{WebUtility.HtmlEncode(currency)}}</td>
                  <td style="padding:10px 8px;border-bottom:1px solid #294154;text-align:right;color:#e5c281;">{{line.LineTotal:N2}} {{WebUtility.HtmlEncode(currency)}}</td>
                </tr>
                """));
        return $$"""
            <div style="margin:24px 0;">
              <h2 style="color:#f3e9d2;font-size:16px;">Sản phẩm trong đơn</h2>
              <table style="width:100%;border-collapse:collapse;font-size:13px;">
                <thead><tr style="color:#8eb9d6;text-align:left;">
                  <th style="padding:8px;">Sản phẩm</th><th style="padding:8px;text-align:center;">SL</th>
                  <th style="padding:8px;text-align:right;">Đơn giá</th><th style="padding:8px;text-align:right;">Thành tiền</th>
                </tr></thead>
                <tbody>{{rows}}</tbody>
              </table>
            </div>
            """;
    }
}
