namespace CosmicWiki.Payments.Api.Security;

public sealed class StripeWebhookOptions
{
    public const string SectionName = "StripeWebhook";

    public string SigningSecret { get; set; } = string.Empty;
    public int TimestampToleranceSeconds { get; set; } = 300;
    public int MaxBodyBytes { get; set; } = 1_048_576;
}
