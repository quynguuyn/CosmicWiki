using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace CosmicWiki.Api.Common.Middleware;

public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Content-Security-Policy"] =
            "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self'";
        if (!environment.IsDevelopment() && context.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] =
                "max-age=31536000; includeSubDomains";
        }

        await next(context);
    }
}
