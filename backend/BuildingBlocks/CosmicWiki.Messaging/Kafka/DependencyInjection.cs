using Confluent.Kafka;
using CosmicWiki.Messaging.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Messaging.Kafka;

public static class DependencyInjection
{
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(
            configuration.GetSection(
                KafkaOptions.SectionName));

        var options = configuration
            .GetSection(KafkaOptions.SectionName)
            .Get<KafkaOptions>()
            ?? new KafkaOptions();

        if (string.IsNullOrWhiteSpace(options.BootstrapServers))
        {
            throw new InvalidOperationException(
                "Kafka BootstrapServers is not configured.");
        }

        var environment =
            configuration["ASPNETCORE_ENVIRONMENT"] ??
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
            "Production";

        if (!string.Equals(
                environment,
                "Development",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                options.SecurityProtocol,
                nameof(SecurityProtocol.SaslSsl),
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                options.SecurityProtocol,
                nameof(SecurityProtocol.Ssl),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Kafka TLS is required outside Development.");
        }

        _ = CreateClientConfig(options);

        services.AddSingleton<IProducer<string, string>>(_ =>
        {
            var config = new ProducerConfig(
                CreateClientConfig(options))
            {
                EnableIdempotence = options.EnableIdempotence,
                Acks = Acks.All
            };

            return new ProducerBuilder<string, string>(
                    config)
                .Build();
        });

        services.AddSingleton<KafkaConsumerFactory>();
        services.AddSingleton<IEventBus, KafkaEventBus>();

        return services;
    }

    internal static ClientConfig CreateClientConfig(KafkaOptions options)
    {
        if (!Enum.TryParse<SecurityProtocol>(
                options.SecurityProtocol,
                ignoreCase: true,
                out var securityProtocol))
        {
            throw new InvalidOperationException(
                $"Unsupported Kafka security protocol '{options.SecurityProtocol}'.");
        }

        var config = new ClientConfig
        {
            BootstrapServers = options.BootstrapServers,
            ClientId = options.ClientId,
            SecurityProtocol = securityProtocol
        };

        if (securityProtocol is SecurityProtocol.SaslSsl or SecurityProtocol.SaslPlaintext)
        {
            if (string.IsNullOrWhiteSpace(options.SaslUsername) ||
                string.IsNullOrWhiteSpace(options.SaslPassword))
            {
                throw new InvalidOperationException(
                    "Kafka SASL username and password are required for SASL security protocols.");
            }

            config.SaslUsername = options.SaslUsername;
            config.SaslPassword = options.SaslPassword;

            if (!Enum.TryParse<SaslMechanism>(
                    options.SaslMechanism ?? "ScramSha512",
                    ignoreCase: true,
                    out var saslMechanism))
            {
                throw new InvalidOperationException(
                    $"Unsupported Kafka SASL mechanism '{options.SaslMechanism}'.");
            }

            config.SaslMechanism = saslMechanism;
        }

        if (securityProtocol is SecurityProtocol.SaslSsl or SecurityProtocol.Ssl)
        {
            if (string.IsNullOrWhiteSpace(options.SslCaLocation))
            {
                throw new InvalidOperationException(
                    "Kafka SslCaLocation is required for TLS security protocols.");
            }

            config.SslCaLocation = options.SslCaLocation;
            config.SslCertificateLocation = options.SslCertificateLocation;
            config.SslKeyLocation = options.SslKeyLocation;
        }

        return config;
    }
}