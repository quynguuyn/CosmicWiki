using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmicWiki.Visit.Infrastructure.Persistence;

public sealed class VisitDbContextFactory : IDesignTimeDbContextFactory<VisitDbContext>
{
    public VisitDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__VisitDatabase")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__VisitDatabase must be set for EF design-time operations.");

        var optionsBuilder = new DbContextOptionsBuilder<VisitDbContext>();
        optionsBuilder.UseOracle(connectionString);
        return new VisitDbContext(optionsBuilder.Options);
    }
}
