using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Caching;
using CosmicWiki.Messaging.Kafka;
using CosmicWiki.Product.Api.Authorization;
using CosmicWiki.Product.Application;
using CosmicWiki.Product.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

builder.Services.AddProductApplication();
builder.Services.AddProductInfrastructure(builder.Configuration);
builder.Services.AddCosmicWikiCaching(builder.Configuration);
builder.Services.AddKafkaMessaging(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    
    // Wait for database to be ready
    var maxRetries = 30;
    var retryDelay = TimeSpan.FromSeconds(2);
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            await dbContext.Database.CanConnectAsync();
            break;
        }
        catch
        {
            if (i == maxRetries - 1) throw;
            await Task.Delay(retryDelay);
        }
    }
    
    await dbContext.Database.MigrateAsync();
}

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
app.MapHealthChecks("/health/live");

app.Run();
