using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmicWiki.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContextFactory
    : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(
        string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "COSMICWIKI_CATALOG_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Environment variable " +
                "'COSMICWIKI_CATALOG_CONNECTION' " +
                "is not configured.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<CatalogDbContext>();

        optionsBuilder.UseOracle(connectionString);

        return new CatalogDbContext(
            optionsBuilder.Options);
    }
}