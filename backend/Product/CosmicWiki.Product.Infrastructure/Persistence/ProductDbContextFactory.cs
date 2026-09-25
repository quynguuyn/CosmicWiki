using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmicWiki.Product.Infrastructure.Persistence;

public sealed class ProductDbContextFactory
    : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__ProductDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__ProductDatabase must be set for EF design-time operations.");

        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();
        optionsBuilder.UseOracle(connectionString);
        return new ProductDbContext(optionsBuilder.Options);
    }
}
