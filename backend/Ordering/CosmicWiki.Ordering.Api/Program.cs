using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Messaging.Kafka;
using CosmicWiki.Ordering.Api.Authorization;
using CosmicWiki.Ordering.Api.Health;
using CosmicWiki.Ordering.Api.Internal;
using CosmicWiki.Ordering.Api.RateLimiting;
using CosmicWiki.Ordering.Api.Serialization;
using CosmicWiki.Ordering.Application;
using CosmicWiki.Ordering.Infrastructure;
using CosmicWiki.Ordering.Infrastructure.Persistence;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

if (!builder.Environment.IsDevelopment())
{
    var serviceKey = builder.Configuration["InternalOrdering:ServiceKey"] ??
        Environment.GetEnvironmentVariable("ORDERING_SERVICE_KEY");
    var kafkaProtocol = builder.Configuration["Kafka:SecurityProtocol"];

    if (string.IsNullOrWhiteSpace(serviceKey) ||
        serviceKey.Length < 32 ||
        serviceKey is "[SECRET_FROM_ENV]" or "local-ordering-key" ||
        string.Equals(kafkaProtocol, "Plaintext", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Production Ordering service credentials and authenticated Kafka are required.");
    }
}

builder.Services.AddOrderingApplication();
builder.Services.AddOrderingInfrastructure(builder.Configuration);
builder.Services.AddCosmicWikiCaching(builder.Configuration);
builder.Services.AddKafkaMessaging(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck<OrderingDatabaseHealthCheck>("ordering-database")
    .AddCheck<KafkaHealthCheck>("kafka");
builder.Services.AddOrderingAuthorization();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter()));
builder.Services.AddSignalR();
builder.Services.AddInternalRateLimiting();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is not configured.");
var audience = jwtSection["Audience"]
    ?? throw new InvalidOperationException("JWT audience is not configured.");
var publicKeyPath = jwtSection["PublicKeyPath"]
    ?? throw new InvalidOperationException("JWT public key path is not configured.");
var publicKeyPem = File.ReadAllText(
    Path.Combine(builder.Environment.ContentRootPath, publicKeyPath));
using var rsa = RSA.Create();
rsa.ImportFromPem(publicKeyPem);
var rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(false));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/api/notifications/hub"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<InternalOrderingOptions>(
    builder.Configuration.GetSection(InternalOrderingOptions.SectionName));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderNotificationsHub>("/api/notifications/hub");
app.MapHealthChecks("/health");
app.MapHealthChecks(
    "/health/live",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
app.Run();
