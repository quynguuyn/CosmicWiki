using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmicWiki.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContextFactory
    : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "COSMICWIKI_ORDERING_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Environment variable 'COSMICWIKI_ORDERING_CONNECTION' is not configured.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<OrderingDbContext>();
        optionsBuilder.UseOracle(connectionString);

        return new OrderingDbContext(optionsBuilder.Options);
    }
}
