using System.Security.Cryptography;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Discovery.Api.Authorization;
using CosmicWiki.Discovery.Application;
using CosmicWiki.Discovery.Infrastructure.Persistence;
using CosmicWiki.Messaging.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

builder.Services.AddDiscoveryApplication();
builder.Services.AddDiscoveryInfrastructure(
    builder.Configuration);
builder.Services.AddKafkaMessaging(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddControllers();

var jwtSection = builder.Configuration.GetSection("Jwt");

var issuer =
    jwtSection["Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is not configured.");

var audience =
    jwtSection["Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is not configured.");

var publicKeyPath =
    jwtSection["PublicKeyPath"]
    ?? throw new InvalidOperationException(
        "JWT public key path is not configured.");

var fullPublicKeyPath = Path.Combine(
    builder.Environment.ContentRootPath,
    publicKeyPath);

var publicKeyPem =
    File.ReadAllText(fullPublicKeyPath);

using var rsa = RSA.Create();

rsa.ImportFromPem(publicKeyPem);

var rsaSecurityKey =
    new RsaSecurityKey(
        rsa.ExportParameters(false));

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
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
    });

builder.Services.AddDiscoveryAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<DiscoveryDbContext>();

    await dbContext.Database.MigrateAsync();
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live");

app.Run();
