using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CosmicWiki.Auth.Api.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred.");

        var statusCode = exception switch
        {
            UnauthorizedAccessException
                => StatusCodes.Status401Unauthorized,

            ArgumentException
                => StatusCodes.Status400BadRequest,

            InvalidOperationException
                => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status500InternalServerError
        };

        var title = statusCode switch
        {
            StatusCodes.Status401Unauthorized
                => "Unauthorized",

            StatusCodes.Status400BadRequest
                => "Bad Request",

            StatusCodes.Status409Conflict
                => "Conflict",

            _ => "Internal Server Error"
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}