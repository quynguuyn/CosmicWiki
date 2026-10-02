using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using CosmicWiki.Auth.Api.Authorization;
using CosmicWiki.Auth.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CosmicWiki.Auth.Api.Exceptions;
using CosmicWiki.Auth.Api.RateLimiting;
using CosmicWiki.Auth.Infrastructure.Persistence;
using CosmicWiki.Auth.Infrastructure.Persistence.Seed;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    var privateKeyPem =
        builder.Configuration["Jwt:PrivateKeyPem"] ??
        Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY_PEM");

    if (string.IsNullOrWhiteSpace(privateKeyPem))
    {
        throw new InvalidOperationException(
            "JWT private key material must be configured outside Development.");
    }
}

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT access token."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

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
    });

builder.Services.AddAuthRateLimiting();

builder.Services.AddAuthAuthorization();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; frame-ancestors 'none'; object-src 'none'; base-uri 'self'";
    await next();
});

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<AuthDbContext>();

    await dbContext.Database.MigrateAsync();

    await AuthDataSeeder.SeedAsync(
        dbContext);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();