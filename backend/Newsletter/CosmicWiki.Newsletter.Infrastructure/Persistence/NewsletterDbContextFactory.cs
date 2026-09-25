using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmicWiki.Newsletter.Infrastructure.Persistence;

public sealed class NewsletterDbContextFactory
    : IDesignTimeDbContextFactory<NewsletterDbContext>
{
    public NewsletterDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__NewsletterDatabase")
            ?? Environment.GetEnvironmentVariable("COSMICWIKI_NEWSLETTER_CONNECTION")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__NewsletterDatabase must be set for EF design-time operations.");

        var optionsBuilder = new DbContextOptionsBuilder<NewsletterDbContext>();
        optionsBuilder.UseOracle(connectionString);
        return new NewsletterDbContext(optionsBuilder.Options);
    }
}
