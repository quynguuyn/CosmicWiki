using System.Security.Cryptography;
using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using CosmicWiki.Auth.Domain.Enums;
using CosmicWiki.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CosmicWiki.Auth.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly AuthDbContext _dbContext;
    private readonly int _persistentExpirationDays;
    private readonly int _sessionExpirationHours;

    public RefreshTokenService(
        AuthDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        var configuredExpirationDays =
            configuration["RefreshToken:ExpirationDays"];

        if (configuredExpirationDays is null)
        {
            _persistentExpirationDays = 30;
        }
        else if (
            int.TryParse(configuredExpirationDays, out var expirationDays)
            && expirationDays > 0)
        {
            _persistentExpirationDays = expirationDays;
        }
        else
        {
            throw new InvalidOperationException(
                "Refresh token expiration days must be a positive integer.");
        }

        var configuredSessionHours = configuration["RefreshToken:SessionExpirationHours"];
        if (configuredSessionHours is null)
        {
            _sessionExpirationHours = 24;
        }
        else if (int.TryParse(configuredSessionHours, out var sessionHours) && sessionHours > 0)
        {
            _sessionExpirationHours = sessionHours;
        }
        else
        {
            throw new InvalidOperationException(
                "Refresh token session expiration hours must be a positive integer.");
        }
    }

    public async Task<RefreshTokenCreation> CreateAsync(
        Guid userId,
        bool rememberMe,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens
            .Where(x => x.ExpiresAtUtc <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);

        var rawToken = GenerateToken();
        var expiresAtUtc = rememberMe
            ? DateTime.UtcNow.AddDays(_persistentExpirationDays)
            : DateTime.UtcNow.AddHours(_sessionExpirationHours);
        var token = RefreshToken.Create(
            userId,
            Guid.NewGuid(),
            HashToken(rawToken),
            expiresAtUtc,
            rememberMe);

        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenCreation(rawToken, expiresAtUtc, rememberMe);
    }

    public async Task<RefreshTokenRotation?> RotateAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(token);
        var currentToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (currentToken is null)
        {
            return null;
        }

        if (currentToken.RevokedAtUtc is not null)
        {
            await RevokeFamilyAsync(
                currentToken.FamilyId,
                cancellationToken);
            return null;
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                x => x.Id == currentToken.UserId,
                cancellationToken);

        if (!currentToken.IsActive(DateTime.UtcNow)
            || user?.Status != UserStatus.Active)
        {
            await RevokeFamilyAsync(
                currentToken.FamilyId,
                cancellationToken);
            return null;
        }

        var replacementToken = GenerateToken();
        var replacementHash = HashToken(replacementToken);

        currentToken.Revoke(replacementHash);
        _dbContext.RefreshTokens.Add(
            RefreshToken.Create(
                currentToken.UserId,
                currentToken.FamilyId,
                replacementHash,
                currentToken.ExpiresAtUtc,
                currentToken.RememberMe));

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            await RevokeFamilyAsync(
                currentToken.FamilyId,
                cancellationToken);
            return null;
        }

        return new RefreshTokenRotation(
            currentToken.UserId,
            replacementToken,
            currentToken.ExpiresAtUtc,
            currentToken.RememberMe);
    }

    public async Task RevokeAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(token);
        var refreshToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (refreshToken is null)
        {
            return;
        }

        await RevokeFamilyAsync(
            refreshToken.FamilyId,
            cancellationToken);
    }

    private async Task RevokeFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(x => x.FamilyId == familyId && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.Revoke();
        }

        if (activeTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static string HashToken(string token) =>
        Convert.ToHexString(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
