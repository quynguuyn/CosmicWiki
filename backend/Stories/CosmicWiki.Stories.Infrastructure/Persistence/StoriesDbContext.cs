using CosmicWiki.Stories.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Stories.Infrastructure.Persistence;

public sealed class StoriesDbContext(DbContextOptions<StoriesDbContext> options) : DbContext(options)
{
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<SupportConversation> SupportConversations => Set<SupportConversation>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    public DbSet<SupportAgentPresence> SupportAgentPresence => Set<SupportAgentPresence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("COSMICWIKI_STORIES");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoriesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
