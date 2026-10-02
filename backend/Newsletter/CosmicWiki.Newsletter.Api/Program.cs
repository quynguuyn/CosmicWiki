using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CosmicWiki.Newsletter.Application;
using CosmicWiki.Newsletter.Infrastructure;
using CosmicWiki.Newsletter.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddNewsletterApplication()
    .AddNewsletterInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<NewsletterExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("newsletter-subscribe", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NewsletterDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.MapControllers();
app.Run();

public sealed class NewsletterExceptionHandler(
    ILogger<NewsletterExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FluentValidation.ValidationException validationException)
        {
            return false;
        }

        logger.LogInformation(
            "Newsletter request validation failed: {Errors}",
            validationException.Message);
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await Results.ValidationProblem(
            validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(error => error.ErrorMessage).ToArray()))
            .ExecuteAsync(httpContext);
        return true;
    }
}
