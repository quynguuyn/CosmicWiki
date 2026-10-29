using Microsoft.EntityFrameworkCore;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Messaging.Kafka;
using CosmicWiki.Payments.Api.Health;
using CosmicWiki.Payments.Api.Internal;
using CosmicWiki.Payments.Api.RateLimiting;
using CosmicWiki.Payments.Api.Security;
using CosmicWiki.Payments.Application;
using CosmicWiki.Payments.Application.Abstractions;
using CosmicWiki.Payments.Infrastructure;
using CosmicWiki.Payments.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

if (!builder.Environment.IsDevelopment())
{
    var webhookSecret = builder.Configuration["PaymentWebhook:SharedSecret"];
    var stripeSecret = builder.Configuration["StripeWebhook:SigningSecret"];
    var orderingKey = builder.Configuration["OrderingService:ServiceKey"] ??
        Environment.GetEnvironmentVariable("ORDERING_SERVICE_KEY");
    var orderingUrl = builder.Configuration["OrderingService:BaseUrl"];
    var allowHttp = builder.Configuration.GetValue(
        "OrderingService:AllowHttp",
        false);

    if (string.IsNullOrWhiteSpace(webhookSecret) ||
        webhookSecret.Length < 32 ||
        webhookSecret is "[SECRET_FROM_ENV]" or "local-payment-secret" ||
        string.IsNullOrWhiteSpace(stripeSecret) ||
        stripeSecret.Length < 32 ||
        stripeSecret == "[SECRET_FROM_ENV]" ||
        string.IsNullOrWhiteSpace(orderingKey) ||
        orderingKey.Length < 32 ||
        orderingKey is "[SECRET_FROM_ENV]" or "local-ordering-key" ||
        !Uri.TryCreate(orderingUrl, UriKind.Absolute, out var uri) ||
        uri.Scheme != Uri.UriSchemeHttps ||
        allowHttp)
    {
        throw new InvalidOperationException(
            "Production payment secrets and HTTPS OrderingService configuration are required.");
    }
}

builder.Services.AddPaymentsApplication();
builder.Services.AddPaymentsInfrastructure(builder.Configuration);
builder.Services.AddKafkaMessaging(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck<PaymentsDatabaseHealthCheck>("payments-database")
    .AddCheck<PaymentsKafkaHealthCheck>("kafka");
builder.Services.AddHttpClient<IOrderPaymentValidator, OrderPaymentValidator>();
builder.Services.Configure<OrderingServiceOptions>(
    builder.Configuration.GetSection(OrderingServiceOptions.SectionName));
builder.Services.Configure<PaymentWebhookOptions>(
    builder.Configuration.GetSection(PaymentWebhookOptions.SectionName));
builder.Services.AddSingleton<IPaymentWebhookSignatureValidator, PaymentWebhookSignatureValidator>();
builder.Services.Configure<StripeWebhookOptions>(
    builder.Configuration.GetSection(StripeWebhookOptions.SectionName));
builder.Services.AddSingleton<IStripeWebhookSignatureValidator, StripeWebhookSignatureValidator>();
builder.Services.AddControllers();
builder.Services.AddPaymentRateLimiting();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
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
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks(
    "/health/live",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
app.Run();
