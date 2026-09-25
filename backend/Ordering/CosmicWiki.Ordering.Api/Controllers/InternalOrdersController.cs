using System.Security.Cryptography;
using System.Text;
using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CosmicWiki.Ordering.Api.Internal;
using CosmicWiki.Ordering.Api.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("internal/orders")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class InternalOrdersController(
    IOrderPaymentIntentValidator validator,
    IOptions<InternalOrderingOptions> options) : ControllerBase
{
    [HttpGet("{orderId:guid}/payment-intent")]
    [EnableRateLimiting(InternalRateLimitingExtensions.PaymentValidationPolicy)]
    public async Task<IActionResult> ValidatePaymentIntent(
        Guid orderId,
        [FromQuery] string paymentIntentId,
        [FromQuery] decimal amount,
        [FromQuery] string currency,
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";

        var expectedKey = options.Value.ServiceKey;
        if (string.IsNullOrWhiteSpace(expectedKey) ||
            expectedKey == "[SECRET_FROM_ENV]")
        {
            expectedKey = Environment.GetEnvironmentVariable(
                "ORDERING_SERVICE_KEY");
        }

        if (!Request.Headers.TryGetValue("X-Internal-Service-Key", out var serviceKey) ||
            serviceKey.Count != 1 ||
            serviceKey[0]!.Length > 512 ||
            string.IsNullOrWhiteSpace(expectedKey) ||
            !FixedTimeEquals(serviceKey.ToString(), expectedKey))
            return Unauthorized();

        if (orderId == Guid.Empty ||
            string.IsNullOrWhiteSpace(paymentIntentId) ||
            paymentIntentId.Length > 200 ||
            amount < 0 ||
            string.IsNullOrWhiteSpace(currency) ||
            currency.Length != 3 ||
            currency.Any(character => character is < 'A' or > 'Z' &&
                character is < 'a' or > 'z'))
        {
            return BadRequest();
        }

        var valid = await validator.IsPaymentIntentValidAsync(
            orderId,
            paymentIntentId,
            amount,
            currency,
            cancellationToken);

        return valid ? Ok() : NotFound();
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
            CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
