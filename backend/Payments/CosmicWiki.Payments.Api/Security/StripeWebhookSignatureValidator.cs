using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Payments.Api.Security;

public interface IStripeWebhookSignatureValidator
{
    Task<PaymentWebhookSignatureResult> ValidateAsync(HttpRequest request, CancellationToken cancellationToken);
}

public sealed class StripeWebhookSignatureValidator(
    IOptions<StripeWebhookOptions> options) : IStripeWebhookSignatureValidator
{
    public async Task<PaymentWebhookSignatureResult> ValidateAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.SigningSecret) ||
            settings.TimestampToleranceSeconds < 0 ||
            settings.MaxBodyBytes <= 0)
            return PaymentWebhookSignatureResult.NotConfigured;

        if (request.ContentLength is > 0 &&
            request.ContentLength > settings.MaxBodyBytes)
            return PaymentWebhookSignatureResult.Invalid;

        if (!request.Headers.TryGetValue("Stripe-Signature", out var signatureValues) ||
            signatureValues.Count != 1 ||
            !TryParseSignature(signatureValues[0]!, out var timestamp, out var signatures))
            return PaymentWebhookSignatureResult.Invalid;

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (timestamp < now - settings.TimestampToleranceSeconds ||
            timestamp > now + settings.TimestampToleranceSeconds)
            return PaymentWebhookSignatureResult.Invalid;

        request.EnableBuffering();
        request.Body.Position = 0;
        var payload = await ReadBodyAsync(request.Body, settings.MaxBodyBytes, cancellationToken);
        request.Body.Position = 0;
        if (payload is null || payload.Length == 0)
            return PaymentWebhookSignatureResult.Invalid;

        var signedPayload = Encoding.UTF8.GetBytes(
            $"{timestamp}.{Encoding.UTF8.GetString(payload)}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.SigningSecret));
        var expected = hmac.ComputeHash(signedPayload);
        foreach (var signature in signatures)
        {
            try
            {
                var provided = Convert.FromHexString(signature);
                if (provided.Length == expected.Length &&
                    CryptographicOperations.FixedTimeEquals(provided, expected))
                    return PaymentWebhookSignatureResult.Valid;
            }
            catch (FormatException)
            {
            }
        }

        return PaymentWebhookSignatureResult.Invalid;
    }

    private static bool TryParseSignature(
        string header,
        out long timestamp,
        out IReadOnlyCollection<string> signatures)
    {
        timestamp = 0;
        var signatureValues = new List<string>();
        foreach (var value in header.Split(',', StringSplitOptions.TrimEntries))
        {
            var separator = value.IndexOf('=');
            if (separator <= 0 || separator == value.Length - 1)
                continue;
            var key = value[..separator];
            var content = value[(separator + 1)..];
            if (key == "t")
                long.TryParse(content, out timestamp);
            else if (key == "v1")
                signatureValues.Add(content);
        }
        signatures = signatureValues;
        return timestamp > 0 && signatureValues.Count > 0;
    }

    private static async Task<byte[]?> ReadBodyAsync(
        Stream body,
        int maxBodyBytes,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[81920];
        await using var copy = new MemoryStream();
        while (true)
        {
            var bytesRead = await body.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
                return copy.ToArray();
            if (copy.Length + bytesRead > maxBodyBytes)
                return null;
            await copy.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }
    }
}
