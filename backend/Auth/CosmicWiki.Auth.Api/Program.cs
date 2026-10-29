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
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    allowedOrigins = ["http://localhost:4200", "https://localhost:4200"];
}

if (!builder.Environment.IsDevelopment() &&
    (allowedOrigins.Length == 0 ||
     allowedOrigins.Any(origin =>
     {
         var normalizedOrigin = origin.TrimEnd('/');
         return !Uri.TryCreate(normalizedOrigin, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps ||
                uri.AbsolutePath != "/" ||
                !string.IsNullOrEmpty(uri.Query) ||
                !string.IsNullOrEmpty(uri.Fragment);
     })))
{
    throw new InvalidOperationException(
        "Production CORS must include at least one HTTPS frontend origin without a path.");
}

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

    var passwordResetFrontendUrl = builder.Configuration["PasswordResetEmail:FrontendUrl"];
    if (!builder.Configuration.GetValue<bool>("PasswordResetEmail:Enabled") ||
        string.IsNullOrWhiteSpace(builder.Configuration["PasswordResetEmail:SmtpHost"]) ||
        string.IsNullOrWhiteSpace(builder.Configuration["PasswordResetEmail:FromAddress"]) ||
        !Uri.TryCreate(passwordResetFrontendUrl, UriKind.Absolute, out var resetUri) ||
        resetUri.Scheme != Uri.UriSchemeHttps)
    {
        throw new InvalidOperationException(
            "Password reset email delivery, SMTP settings, and an HTTPS frontend URL must be configured outside Development.");
    }
}

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins.Select(origin => origin.TrimEnd('/')).ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        });
});

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
        dbContext,
        scope.ServiceProvider.GetRequiredService<CosmicWiki.Auth.Application.Abstractions.Persistence.IPasswordHasher>(),
        builder.Configuration);
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

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live");

app.Run();