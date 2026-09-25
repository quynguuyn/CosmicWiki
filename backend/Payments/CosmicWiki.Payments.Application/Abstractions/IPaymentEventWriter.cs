using CosmicWiki.Payments.Domain.Entities;

namespace CosmicWiki.Payments.Application.Abstractions;

public interface IPaymentEventWriter
{
    void AddStatusChanged(Payment payment);
}
