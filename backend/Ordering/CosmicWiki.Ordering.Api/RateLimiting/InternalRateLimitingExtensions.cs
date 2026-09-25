using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmicWiki.Ordering.Api.RateLimiting;

public static class InternalRateLimitingExtensions
{
    public const string PaymentValidationPolicy = "internal-payment-validation";

    public static IServiceCollection AddInternalRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(
                PaymentValidationPolicy,
                context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
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
                        detail = "Too many internal requests. Please try again later.",
                        traceId = context.HttpContext.TraceIdentifier
                    },
                    cancellationToken);
            };
        });

        return services;
    }
}
