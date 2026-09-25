using System.Security.Cryptography;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Caching;
using CosmicWiki.Messaging.Kafka;
using CosmicWiki.Product.Api.Authorization;
using CosmicWiki.Product.Application;
using CosmicWiki.Product.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProductApplication();
builder.Services.AddProductInfrastructure(builder.Configuration);
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

builder.Services.AddProductAuthorization();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
