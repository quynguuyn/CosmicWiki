using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmicWiki.Payments.Api.RateLimiting;

public static class PaymentRateLimitingExtensions
{
    public const string WebhookPolicy = "payment-webhook";

    public static IServiceCollection AddPaymentRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(
                WebhookPolicy,
                context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        title = "Too Many Requests",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = "Too many webhook requests. Please try again later.",
                        traceId = context.HttpContext.TraceIdentifier
                    },
                    cancellationToken);
            };
        });

        return services;
    }
}
