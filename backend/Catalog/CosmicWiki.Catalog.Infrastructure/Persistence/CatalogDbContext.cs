using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext : DbContext, ICatalogUnitOfWork
{
    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Star> Stars =>
        Set<Star>();

    public DbSet<AstronomicalObject> AstronomicalObjects =>
        Set<AstronomicalObject>();

    public DbSet<AstronomicalImage> AstronomicalImages =>
        Set<AstronomicalImage>();

    public DbSet<CatalogOutboxMessage> OutboxMessages => Set<CatalogOutboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(
            "COSMICWIKI_CATALOG");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly);

        modelBuilder.Entity<CatalogOutboxMessage>(builder =>
        {
            builder.ToTable("OUTBOX_MESSAGES");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Topic).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.AggregateId).IsRequired();
            builder.HasIndex(x => x.EventId).IsUnique();
            builder.HasIndex(x => x.PublishedAtUtc);
        });

        base.OnModelCreating(modelBuilder);
    }
}
