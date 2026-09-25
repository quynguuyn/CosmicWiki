using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Ordering.Infrastructure.Payment;

public sealed class StripePaymentProvider(
    HttpClient httpClient,
    IOptions<PaymentProviderOptions> options) : IPaymentProvider
{
    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.SecretKey))
            throw new InvalidOperationException("Stripe secret key is not configured.");

        var minorAmount = decimal.ToInt64(
            decimal.Round(
                IsZeroDecimalCurrency(currency) ? amount : amount * 100m,
                0,
                MidpointRounding.AwayFromZero));
        if (minorAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            new Uri(new Uri(settings.ApiBaseUrl.TrimEnd('/') + "/"), "v1/payment_intents"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.SecretKey);
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["amount"] = minorAmount.ToString(CultureInfo.InvariantCulture),
            ["currency"] = currency.Trim().ToLowerInvariant(),
            ["metadata[order_id]"] = orderId.ToString("D"),
            ["automatic_payment_methods[enabled]"] = "true",
            ["automatic_payment_methods[allow_redirects]"] = "never"
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Stripe payment intent creation failed with HTTP {(int)response.StatusCode}.");

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        var paymentIntentId = root.GetProperty("id").GetString();
        var status = root.GetProperty("status").GetString();
        var clientSecret = root.GetProperty("client_secret").GetString();
        if (string.IsNullOrWhiteSpace(paymentIntentId) || string.IsNullOrWhiteSpace(status))
            throw new InvalidOperationException("Stripe returned an incomplete payment intent.");

        return new PaymentIntentResult(paymentIntentId, status, clientSecret);
    }

    private static bool IsZeroDecimalCurrency(string currency) =>
        currency.Trim().ToUpperInvariant() is
            "BIF" or "CLP" or "DJF" or "GNF" or "JPY" or "KMF" or
            "KRW" or "MGA" or "PYG" or "RWF" or "UGX" or "VND" or
            "VUV" or "XAF" or "XOF" or "XPF";
}
