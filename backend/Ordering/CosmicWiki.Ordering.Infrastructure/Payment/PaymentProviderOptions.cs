namespace CosmicWiki.Ordering.Infrastructure.Payment;

public sealed class PaymentProviderOptions
{
    public const string SectionName = "PaymentProvider";

    public string Mode { get; init; } = "Disabled";

    public string? SecretKey { get; init; }

    public string ApiBaseUrl { get; init; } = "https://api.stripe.com";
}
