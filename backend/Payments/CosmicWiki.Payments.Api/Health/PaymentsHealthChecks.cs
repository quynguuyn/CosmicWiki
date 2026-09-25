using Confluent.Kafka;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Kafka;
using CosmicWiki.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Payments.Api.Health;

public sealed class PaymentsDatabaseHealthCheck(
    IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        return await db.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Payments database is unavailable.");
    }
}

public sealed class PaymentsKafkaHealthCheck(
    IOptions<KafkaOptions> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = options.Value.BootstrapServers
            };
            using var adminClient = new AdminClientBuilder(config).Build();
            adminClient.GetMetadata(
                KafkaTopics.PaymentStatusChanged,
                TimeSpan.FromSeconds(2));
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception exception)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Kafka is unavailable.",
                    exception));
        }
    }
}
