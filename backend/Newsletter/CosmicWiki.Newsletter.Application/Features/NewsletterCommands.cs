using CosmicWiki.Newsletter.Application.Abstractions;
using CosmicWiki.Newsletter.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CosmicWiki.Newsletter.Application.Features;

public sealed record SubscribeNewsletterCommand(string Email) : IRequest<SubscribeNewsletterResult>;

public sealed record SubscribeNewsletterResult(bool Created, bool AlreadySubscribed);

public sealed record ConfirmNewsletterSubscriptionCommand(string Token) : IRequest<ConfirmNewsletterResult>;

public sealed record ConfirmNewsletterResult(bool Found, bool Confirmed, bool Expired);

public sealed record UnsubscribeNewsletterCommand(string Token) : IRequest<UnsubscribeNewsletterResult>;

public sealed record UnsubscribeNewsletterResult(bool Found);

public sealed class SubscribeNewsletterValidator : AbstractValidator<SubscribeNewsletterCommand>
{
    public SubscribeNewsletterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
    }
}

public sealed class SubscribeNewsletterCommandHandler(
    INewsletterSubscriptionRepository subscriptions,
    INewsletterDeliveryRepository deliveries,
    INewsletterUnitOfWork unitOfWork,
    IConfirmationTokenService tokens,
    NewsletterOptions options,
    TimeProvider timeProvider)
    : IRequestHandler<SubscribeNewsletterCommand, SubscribeNewsletterResult>
{
    public async Task<SubscribeNewsletterResult> Handle(
        SubscribeNewsletterCommand request,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var email = NewsletterSubscription.NormalizeEmail(request.Email);
        var existing = await subscriptions.GetByEmailAsync(email, cancellationToken);

        if (existing is not null &&
            existing.Status == Domain.Enums.SubscriptionStatus.Confirmed)
        {
            return new SubscribeNewsletterResult(false, true);
        }

        if (existing is not null &&
            existing.Status == Domain.Enums.SubscriptionStatus.PendingConfirmation &&
            !existing.IsConfirmationTokenExpired(now))
        {
            return new SubscribeNewsletterResult(false, true);
        }

        var (rawToken, hash) = tokens.Create();
        var expiresAt = now.Add(options.ConfirmationTokenLifetime);
        NewsletterSubscription subscription;

        if (existing is null)
        {
            subscription = NewsletterSubscription.Create(email, hash, expiresAt, now);
            await subscriptions.AddAsync(subscription, cancellationToken);
        }
        else
        {
            subscription = existing;
            subscription.RenewConfirmation(hash, expiresAt, now);
        }

        var confirmationUrl =
            $"{options.ConfirmationBaseUrl.TrimEnd('/')}/{Uri.EscapeDataString(rawToken)}";
        var body = $"Confirm your CosmicWiki newsletter subscription: {confirmationUrl}";
        var delivery = NewsletterDelivery.CreateConfirmation(subscription, body, now);
        await deliveries.AddAsync(delivery, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubscribeNewsletterResult(true, false);
    }
}

public sealed class ConfirmNewsletterSubscriptionCommandHandler(
    INewsletterSubscriptionRepository subscriptions,
    INewsletterUnitOfWork unitOfWork,
    IConfirmationTokenService tokens,
    TimeProvider timeProvider)
    : IRequestHandler<ConfirmNewsletterSubscriptionCommand, ConfirmNewsletterResult>
{
    public async Task<ConfirmNewsletterResult> Handle(
        ConfirmNewsletterSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetByTokenHashAsync(
            tokens.Hash(request.Token), cancellationToken);
        if (subscription is null)
        {
            return new ConfirmNewsletterResult(false, false, false);
        }

        if (subscription.Status == Domain.Enums.SubscriptionStatus.Confirmed)
        {
            return new ConfirmNewsletterResult(true, true, false);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (subscription.IsConfirmationTokenExpired(now))
        {
            return new ConfirmNewsletterResult(true, false, true);
        }

        subscription.Confirm(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ConfirmNewsletterResult(true, true, false);
    }
}

public sealed class UnsubscribeNewsletterCommandHandler(
    INewsletterSubscriptionRepository subscriptions,
    INewsletterUnitOfWork unitOfWork,
    IConfirmationTokenService tokens,
    TimeProvider timeProvider)
    : IRequestHandler<UnsubscribeNewsletterCommand, UnsubscribeNewsletterResult>
{
    public async Task<UnsubscribeNewsletterResult> Handle(
        UnsubscribeNewsletterCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetByTokenHashAsync(
            tokens.Hash(request.Token), cancellationToken);
        if (subscription is null)
        {
            return new UnsubscribeNewsletterResult(false);
        }

        subscription.Unsubscribe(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UnsubscribeNewsletterResult(true);
    }
}

public sealed class ConfirmNewsletterSubscriptionValidator
    : AbstractValidator<ConfirmNewsletterSubscriptionCommand>
{
    public ConfirmNewsletterSubscriptionValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(256);
    }
}

public sealed class UnsubscribeNewsletterValidator
    : AbstractValidator<UnsubscribeNewsletterCommand>
{
    public UnsubscribeNewsletterValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(256);
    }
}
