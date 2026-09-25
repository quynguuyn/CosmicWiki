using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Payments.Api.Security;

public enum PaymentWebhookSignatureResult
{
    Valid,
    Invalid,
    NotConfigured
}

public interface IPaymentWebhookSignatureValidator
{
    Task<PaymentWebhookSignatureResult> ValidateAsync(HttpRequest request, CancellationToken cancellationToken);
}

public sealed class PaymentWebhookSignatureValidator(
    IOptions<PaymentWebhookOptions> options) : IPaymentWebhookSignatureValidator
{
    public async Task<PaymentWebhookSignatureResult> ValidateAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.SharedSecret) ||
            string.IsNullOrWhiteSpace(settings.SignatureHeader) ||
            (settings.RequireTimestamp && string.IsNullOrWhiteSpace(settings.TimestampHeader)) ||
            settings.TimestampToleranceSeconds < 0 ||
            settings.MaxBodyBytes <= 0)
            return PaymentWebhookSignatureResult.NotConfigured;

        if (request.ContentLength is > 0 &&
            request.ContentLength > settings.MaxBodyBytes)
            return PaymentWebhookSignatureResult.Invalid;

        if (!request.Headers.TryGetValue(settings.SignatureHeader, out var signature) ||
            string.IsNullOrWhiteSpace(signature) ||
            signature.Count != 1)
            return PaymentWebhookSignatureResult.Invalid;

        string? timestamp = null;
        if (settings.RequireTimestamp)
        {
            if (!request.Headers.TryGetValue(settings.TimestampHeader, out var timestampValues) ||
                timestampValues.Count != 1 ||
                !long.TryParse(timestampValues[0], out var timestampSeconds))
                return PaymentWebhookSignatureResult.Invalid;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (timestampSeconds < now - settings.TimestampToleranceSeconds ||
                timestampSeconds > now + settings.TimestampToleranceSeconds)
                return PaymentWebhookSignatureResult.Invalid;

            timestamp = timestampSeconds.ToString();
        }

        request.EnableBuffering();
        request.Body.Position = 0;
        var payload = await ReadBodyAsync(request.Body, settings.MaxBodyBytes, cancellationToken);
        request.Body.Position = 0;
        if (payload is null || payload.Length == 0)
            return PaymentWebhookSignatureResult.Invalid;

        var signedPayload = timestamp is null
            ? payload
            : Encoding.UTF8.GetBytes($"{timestamp}.{Encoding.UTF8.GetString(payload)}");
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.SharedSecret));
        var expected = hmac.ComputeHash(signedPayload);
        var supplied = signature.ToString();
        if (supplied.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            supplied = supplied["sha256=".Length..];

        byte[] provided;
        try
        {
            provided = Convert.FromHexString(supplied);
        }
        catch (FormatException)
        {
            return PaymentWebhookSignatureResult.Invalid;
        }

        return provided.Length == expected.Length &&
               CryptographicOperations.FixedTimeEquals(provided, expected)
            ? PaymentWebhookSignatureResult.Valid
            : PaymentWebhookSignatureResult.Invalid;
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
