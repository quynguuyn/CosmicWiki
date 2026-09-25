using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmicWiki.Stories.Infrastructure.Persistence;

public sealed class StoriesDbContextFactory : IDesignTimeDbContextFactory<StoriesDbContext>
{
    public StoriesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
                "ConnectionStrings__StoriesDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__StoriesDatabase must be set for EF design-time operations.");
        var optionsBuilder = new DbContextOptionsBuilder<StoriesDbContext>();
        optionsBuilder.UseOracle(connectionString);
        return new StoriesDbContext(optionsBuilder.Options);
    }
}
