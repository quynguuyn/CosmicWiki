using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CosmicWiki.Payments.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Payments.Api.Internal;

public sealed class OrderingServiceOptions
{
    public const string SectionName = "OrderingService";

    public string BaseUrl { get; set; } = string.Empty;
    public string ServiceKey { get; set; } = string.Empty;
    public bool AllowHttp { get; set; }
}

public sealed class OrderPaymentValidator(
    HttpClient httpClient,
    IOptions<OrderingServiceOptions> options) : IOrderPaymentValidator
{
    public async Task<bool> IsPaymentIntentValidAsync(
        Guid orderId,
        string paymentIntentId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl.TrimEnd('/');
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && !options.Value.AllowHttp))
        {
            throw new InvalidOperationException(
                "OrderingService BaseUrl must use HTTPS unless AllowHttp is explicitly enabled.");
        }

        var serviceKey = options.Value.ServiceKey;
        if (string.IsNullOrWhiteSpace(serviceKey) ||
            serviceKey == "[SECRET_FROM_ENV]")
        {
            serviceKey = Environment.GetEnvironmentVariable(
                "ORDERING_SERVICE_KEY");
        }

        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            throw new InvalidOperationException(
                "Ordering service credential is not configured.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{baseUrl}/internal/orders/{orderId}/payment-intent" +
            $"?paymentIntentId={Uri.EscapeDataString(paymentIntentId)}" +
            $"&amount={amount.ToString(CultureInfo.InvariantCulture)}" +
            $"&currency={Uri.EscapeDataString(currency)}");
        request.Headers.Add("X-Internal-Service-Key", serviceKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
