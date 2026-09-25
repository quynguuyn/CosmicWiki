using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Api.Common.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred while processing request.");

            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException =>
                StatusCodes.Status400BadRequest,

            KeyNotFoundException =>
                StatusCodes.Status404NotFound,

            InvalidOperationException =>
                StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message;

        var response = new
        {
            title = GetTitle(statusCode),
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal Server Error"
        };
    }
}