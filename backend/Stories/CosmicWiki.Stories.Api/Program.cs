using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using CosmicWiki.Api.Common.Middleware;
using CosmicWiki.Caching;
using CosmicWiki.Stories.Api.Authorization;
using CosmicWiki.Stories.Application;
using CosmicWiki.Stories.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStoriesApplication();
builder.Services.AddStoriesInfrastructure(builder.Configuration);
builder.Services.AddCosmicWikiCaching(builder.Configuration);
builder.Services.AddControllers();

var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");
var publicKeyPath = jwtSection["PublicKeyPath"] ?? throw new InvalidOperationException("JWT public key path is not configured.");
var publicKeyPem = File.ReadAllText(Path.Combine(builder.Environment.ContentRootPath, publicKeyPath));
using var rsa = RSA.Create();
rsa.ImportFromPem(publicKeyPem);
var rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(false));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
builder.Services.AddStoriesAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StoriesDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
