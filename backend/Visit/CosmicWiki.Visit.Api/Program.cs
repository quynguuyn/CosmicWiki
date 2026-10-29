using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Visit.Api.Authorization;
using CosmicWiki.Visit.Api.Health;
using CosmicWiki.Visit.Application;
using CosmicWiki.Visit.Infrastructure;
using CosmicWiki.Visit.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
builder.Services.AddVisitApplication();
builder.Services.AddVisitInfrastructure(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(typeof(CosmicWiki.Visit.Application.DependencyInjection).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddControllers();
builder.Services.AddVisitAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck<VisitDatabaseHealthCheck>("visit-database");

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
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VisitDbContext>();
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks(
    "/health/live",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });
app.Run();

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(validators.Select(x => x.ValidateAsync(context, cancellationToken))))
            .SelectMany(x => x.Errors).Where(x => x is not null).ToList();
        if (failures.Count > 0) throw new ValidationException(failures);
        return await next();
    }
}
