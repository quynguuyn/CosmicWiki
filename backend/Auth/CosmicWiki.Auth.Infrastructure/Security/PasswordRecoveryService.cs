using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using CosmicWiki.Auth.Application.Abstractions.Persistence;
using CosmicWiki.Auth.Domain.Entities;
using CosmicWiki.Auth.Domain.ValueObjects;
using CosmicWiki.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;

namespace CosmicWiki.Auth.Infrastructure.Security;

public sealed class PasswordRecoveryService(
    AuthDbContext db,
    IPasswordHasher passwordHasher,
    IOptions<PasswordResetEmailOptions> options,
    ILogger<PasswordRecoveryService> logger,
    IWebHostEnvironment environment) : IPasswordRecoveryService
{
    private readonly PasswordResetEmailOptions _options = options.Value;
    public async Task<PasswordResetRequestResult> RequestResetAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(
            x => x.Email == Email.Create(email),
            cancellationToken);
        if (user is null)
            return new PasswordResetRequestResult(_options.Enabled, null);

        await db.PasswordResetTokens
            .Where(x => x.ExpiresAtUtc <= DateTime.UtcNow)
            .ExecuteDeleteAsync(cancellationToken);

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        var now = DateTime.UtcNow;
        await db.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAtUtc == null)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(x => x.UsedAtUtc, now),
                cancellationToken);
        db.PasswordResetTokens.Add(PasswordResetToken.Create(
            user.Id,
            tokenHash,
            DateTime.UtcNow.AddHours(1)));
        await db.SaveChangesAsync(cancellationToken);

        var resetUrl = $"{_options.FrontendUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";
        if (!_options.Enabled)
        {
            logger.LogWarning("Password reset email is disabled; reset request for user {UserId} was not delivered.", user.Id);
            return new PasswordResetRequestResult(
                _options.Enabled,
                environment.IsDevelopment() && _options.ExposeDevelopmentLink
                    ? resetUrl
                    : null);
        }

        ValidateConfiguration();
        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = "Đặt lại mật khẩu CosmicWiki",
            Body = $$"""
                <!doctype html><html lang="vi"><body>
                  <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu tài khoản CosmicWiki.</p>
                  <p><a href="{{WebUtility.HtmlEncode(resetUrl)}}">Đặt lại mật khẩu</a></p>
                  <p>Liên kết có hiệu lực trong 60 phút và chỉ sử dụng được một lần. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
                </body></html>
                """,
            IsBodyHtml = true,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(user.Email.Value));
        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        await client.SendMailAsync(message, cancellationToken);
        return new PasswordResetRequestResult(_options.Enabled, null);
    }

    public async Task<bool> ResetPasswordAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        if (newPassword.Length < 8 || newPassword.Length > 256)
            throw new ArgumentException("Password must contain between 8 and 256 characters.", nameof(newPassword));

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var resetToken = await db.PasswordResetTokens
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
        if (resetToken is null || !resetToken.IsUsable(DateTime.UtcNow))
            return false;

        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == resetToken.UserId, cancellationToken);
        if (user is null)
            return false;

        user.ChangePassword(passwordHasher.Hash(newPassword));
        resetToken.MarkUsed();
        var activeRefreshTokens = await db.RefreshTokens
            .Where(x => x.UserId == user.Id && x.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var activeToken in activeRefreshTokens)
            activeToken.Revoke();

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            db.ChangeTracker.Clear();
            return false;
        }
        return true;
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException(
                "Password reset email is enabled but SMTP host and FromAddress are not configured.");
    }
}
