using System.Text.Json;
using CosmicWiki.Payments.Api.Security;
using CosmicWiki.Payments.Application.Features.ProcessWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CosmicWiki.Payments.Api.RateLimiting;

namespace CosmicWiki.Payments.Api.Controllers;

[ApiController]
[Route("api/payments/webhooks")]
[AllowAnonymous]
public sealed class PaymentWebhooksController(
    ISender sender,
    IPaymentWebhookSignatureValidator signatureValidator) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting(PaymentRateLimitingExtensions.WebhookPolicy)]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        var signatureResult = await signatureValidator.ValidateAsync(Request, cancellationToken);
        if (signatureResult == PaymentWebhookSignatureResult.NotConfigured)
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        if (signatureResult != PaymentWebhookSignatureResult.Valid)
            return Unauthorized();

        PaymentWebhookRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<PaymentWebhookRequest>(
                Request.Body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                cancellationToken);
        }
        catch (JsonException)
        {
            return BadRequest("Webhook payload is invalid JSON.");
        }
        if (request is null)
            return BadRequest("Webhook payload is required.");

        var processed = await sender.Send(
            new ProcessPaymentWebhookCommand(
                request.ProviderEventId,
                request.OrderId,
                request.PaymentIntentId,
                request.Amount,
                request.Currency,
                request.Status),
            cancellationToken);

        return processed switch
        {
            PaymentWebhookProcessingResult.Processed => Ok(),
            PaymentWebhookProcessingResult.Duplicate => Ok(new { duplicate = true }),
            _ => BadRequest("Webhook event is invalid.")
        };
    }

    [HttpPost("stripe")]
    [EnableRateLimiting(PaymentRateLimitingExtensions.WebhookPolicy)]
    public async Task<IActionResult> ReceiveStripe(
        [FromServices] IStripeWebhookSignatureValidator stripeSignatureValidator,
        CancellationToken cancellationToken)
    {
        var signatureResult = await stripeSignatureValidator.ValidateAsync(Request, cancellationToken);
        if (signatureResult == PaymentWebhookSignatureResult.NotConfigured)
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        if (signatureResult != PaymentWebhookSignatureResult.Valid)
            return Unauthorized();

        try
        {
            using var document = await JsonDocument.ParseAsync(
                Request.Body,
                cancellationToken: cancellationToken);
            var root = document.RootElement;
            var eventId = root.GetProperty("id").GetString();
            var eventType = root.GetProperty("type").GetString();
            if (string.IsNullOrWhiteSpace(eventId) ||
                eventType is not ("payment_intent.succeeded" or "payment_intent.payment_failed"))
                return BadRequest("Unsupported Stripe event.");

            var paymentIntent = root.GetProperty("data").GetProperty("object");
            var paymentIntentId = paymentIntent.GetProperty("id").GetString();
            var amountMinor = paymentIntent.GetProperty("amount").GetInt64();
            var currency = paymentIntent.GetProperty("currency").GetString();
            var orderIdText = paymentIntent.GetProperty("metadata").GetProperty("order_id").GetString();
            if (string.IsNullOrWhiteSpace(paymentIntentId) ||
                string.IsNullOrWhiteSpace(currency) ||
                !Guid.TryParse(orderIdText, out var orderId))
                return BadRequest("Stripe event is missing payment metadata.");

            var processed = await sender.Send(
                new ProcessPaymentWebhookCommand(
                    eventId,
                    orderId,
                    paymentIntentId,
                    FromStripeMinorUnit(amountMinor, currency),
                    currency.ToUpperInvariant(),
                    eventType == "payment_intent.succeeded" ? "succeeded" : "failed"),
                cancellationToken);

            return processed switch
            {
                PaymentWebhookProcessingResult.Processed => Ok(),
                PaymentWebhookProcessingResult.Duplicate => Ok(new { duplicate = true }),
                _ => BadRequest("Stripe event is invalid.")
            };
        }
        catch (JsonException)
        {
            return BadRequest("Stripe payload is invalid JSON.");
        }
        catch (KeyNotFoundException)
        {
            return BadRequest("Stripe event is missing required fields.");
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Stripe event contains invalid field types.");
        }
    }

    private static decimal FromStripeMinorUnit(long amount, string? currency) =>
        IsZeroDecimalCurrency(currency ?? throw new InvalidOperationException("Stripe event currency is missing."))
            ? amount
            : amount / 100m;

    private static bool IsZeroDecimalCurrency(string currency) =>
        currency.Trim().ToUpperInvariant() is
            "BIF" or "CLP" or "DJF" or "GNF" or "JPY" or "KMF" or
            "KRW" or "MGA" or "PYG" or "RWF" or "UGX" or "VND" or
            "VUV" or "XAF" or "XOF" or "XPF";
}

public sealed record PaymentWebhookRequest(
    string ProviderEventId,
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    string Status);
