using CosmicWiki.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Payments.Infrastructure.Persistence;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProcessedProviderEvent> ProcessedProviderEvents => Set<ProcessedProviderEvent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("COSMICWIKI_PAYMENTS");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}

public sealed class ProcessedProviderEvent
{
    private ProcessedProviderEvent() { }
    public ProcessedProviderEvent(string providerEventId) => ProviderEventId = providerEventId;
    public string ProviderEventId { get; private set; } = null!;
    public DateTime ProcessedAtUtc { get; private set; } = DateTime.UtcNow;
}
