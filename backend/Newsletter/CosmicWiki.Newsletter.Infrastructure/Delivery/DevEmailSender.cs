using CosmicWiki.Newsletter.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace CosmicWiki.Newsletter.Infrastructure.Delivery;

public sealed class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Newsletter development email delivered to {RecipientEmail} with subject {Subject}.",
            message.RecipientEmail,
            message.Subject);
        return Task.CompletedTask;
    }
}
