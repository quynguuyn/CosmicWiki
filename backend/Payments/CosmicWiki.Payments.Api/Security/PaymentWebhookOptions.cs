namespace CosmicWiki.Payments.Api.Security;

public sealed class PaymentWebhookOptions
{
    public const string SectionName = "PaymentWebhook";

    public string SharedSecret { get; set; } = string.Empty;
    public string SignatureHeader { get; set; } = "X-Payment-Signature";
    public string TimestampHeader { get; set; } = "X-Payment-Timestamp";
    public bool RequireTimestamp { get; set; } = true;
    public int TimestampToleranceSeconds { get; set; } = 300;
    public int MaxBodyBytes { get; set; } = 1_048_576;
}
