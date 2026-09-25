using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmicWiki.Auth.Api.RateLimiting;

public static class AuthRateLimitingExtensions
{
    public const string LoginPolicy = "auth-login";

    public static IServiceCollection AddAuthRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(
                LoginPolicy,
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                    limiterOptions.AutoReplenishment = true;
                });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        title = "Too Many Requests",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = "Too many login attempts. Please try again later.",
                        traceId = context.HttpContext.TraceIdentifier
                    },
                    cancellationToken);
            };
        });

        return services;
    }
}