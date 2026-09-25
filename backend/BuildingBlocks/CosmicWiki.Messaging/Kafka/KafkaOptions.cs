namespace CosmicWiki.Messaging.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = null!;
    public string? ClientId { get; set; }
    public string SecurityProtocol { get; set; } = "Plaintext";
    public string? SaslMechanism { get; set; }
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }
    public string? SslCaLocation { get; set; }
    public string? SslCertificateLocation { get; set; }
    public string? SslKeyLocation { get; set; }
    public bool EnableIdempotence { get; set; } = true;
    public string PaymentStatusConsumerGroup { get; set; } =
        "cosmicwiki.ordering.payment-status";
    public int PaymentStatusMaxProcessingAttempts { get; set; } = 3;
    public int PaymentStatusRetryBackoffMilliseconds { get; set; } = 250;
}