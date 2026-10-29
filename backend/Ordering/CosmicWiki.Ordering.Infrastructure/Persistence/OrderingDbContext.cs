using CosmicWiki.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderStatusHistoryEntry> OrderStatusHistory => Set<OrderStatusHistoryEntry>();
    public DbSet<OrderNotification> OrderNotifications => Set<OrderNotification>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("COSMICWIKI_ORDERING");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingDbContext).Assembly);
    }
}
