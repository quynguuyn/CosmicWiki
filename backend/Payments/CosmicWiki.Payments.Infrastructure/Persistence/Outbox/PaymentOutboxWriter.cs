using System.Text.Json;
using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Contracts.Payment;
using CosmicWiki.Payments.Application.Abstractions;
using CosmicWiki.Payments.Domain.Entities;

namespace CosmicWiki.Payments.Infrastructure.Persistence.Outbox;

public sealed class PaymentOutboxWriter(PaymentDbContext dbContext) : IPaymentEventWriter
{
    public void AddStatusChanged(Payment payment)
    {
        var status = payment.Status.ToString().ToLowerInvariant();
        var integrationEvent = new PaymentStatusChangedIntegrationEvent(
            Guid.NewGuid(),
            1,
            DateTime.UtcNow,
            payment.Id,
            payment.OrderId,
            payment.PaymentIntentId,
            payment.Amount,
            payment.Currency,
            status);

        dbContext.OutboxMessages.Add(OutboxMessage.Create(
            integrationEvent.EventId,
            integrationEvent.AggregateId,
            KafkaTopics.PaymentStatusChanged,
            typeof(PaymentStatusChangedIntegrationEvent).AssemblyQualifiedName!,
            JsonSerializer.Serialize(integrationEvent),
            integrationEvent.OccurredAtUtc));
    }
}
