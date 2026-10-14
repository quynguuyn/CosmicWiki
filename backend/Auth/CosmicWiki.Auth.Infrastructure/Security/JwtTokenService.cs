using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CosmicWiki.Auth.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly RsaSecurityKey _signingKey;

    public JwtTokenService(
        IConfiguration configuration)
    {
        _configuration = configuration;

        var jwtSection = _configuration.GetSection("Jwt");

        var privateKeyPem =
            jwtSection["PrivateKeyPem"]
            ?? Environment.GetEnvironmentVariable("JWT_PRIVATE_KEY_PEM")
            ?? throw new InvalidOperationException(
                "JWT private key material is not configured.");

        privateKeyPem = privateKeyPem.Replace("\\n", Environment.NewLine);

        var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        _signingKey = new RsaSecurityKey(rsa);
    }

    public string GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> permissions)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        var expirationMinutes = int.Parse(
            jwtSection["ExpirationMinutes"]
            ?? throw new InvalidOperationException(
                "JWT expiration minutes are not configured."));

        var signingCredentials = new SigningCredentials(
            _signingKey,
            SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.UniqueName,
                user.Username.Value),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email.Value),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(
                JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow
                    .ToUnixTimeSeconds()
                    .ToString(),
                ClaimValueTypes.Integer64)
        };

        foreach (var permission in permissions)
        {
            claims.Add(
                new Claim(
                    "permission",
                    permission));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}