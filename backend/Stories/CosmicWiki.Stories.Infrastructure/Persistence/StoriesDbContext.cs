using CosmicWiki.Stories.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Stories.Infrastructure.Persistence;

public sealed class StoriesDbContext(DbContextOptions<StoriesDbContext> options) : DbContext(options)
{
    public DbSet<Story> Stories => Set<Story>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("COSMICWIKI_STORIES");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoriesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
