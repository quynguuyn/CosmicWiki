namespace CosmicWiki.Newsletter.Application;

public sealed class NewsletterOptions
{
    public string ConfirmationBaseUrl { get; set; } =
        "http://localhost:5000/api/newsletter/confirm";

    public TimeSpan ConfirmationTokenLifetime { get; set; } =
        TimeSpan.FromHours(24);

    public TimeSpan DeliveryPollInterval { get; set; } =
        TimeSpan.FromSeconds(15);

    public int DeliveryBatchSize { get; set; } = 25;
    public int MaxDeliveryAttempts { get; set; } = 5;
}
