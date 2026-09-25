using CosmicWiki.Catalog.Infrastructure.Persistence;
using System.Security.Cryptography;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Caching;
using CosmicWiki.Catalog.Api.Authorization;
using CosmicWiki.Catalog.Application;
using CosmicWiki.Messaging.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCatalogApplication();
builder.Services.AddCatalogInfrastructure(builder.Configuration);
builder.Services.AddCosmicWikiCaching(builder.Configuration);
builder.Services.AddKafkaMessaging(builder.Configuration);

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

builder.Services.AddCatalogAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();
