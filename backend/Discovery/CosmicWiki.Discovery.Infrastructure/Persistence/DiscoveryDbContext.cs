using CosmicWiki.Discovery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Discovery.Infrastructure.Persistence;

public sealed class DiscoveryDbContext : DbContext
{
    public DiscoveryDbContext(
        DbContextOptions<DiscoveryDbContext> options)
        : base(options)
    {
    }

    public DbSet<SearchDocument> SearchDocuments =>
        Set<SearchDocument>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(
            "COSMICWIKI_DISCOVERY");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DiscoveryDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}