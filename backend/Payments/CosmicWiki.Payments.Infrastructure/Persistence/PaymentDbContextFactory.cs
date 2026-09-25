using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CosmicWiki.Payments.Infrastructure.Persistence;

public sealed class PaymentDbContextFactory
    : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "COSMICWIKI_PAYMENTS_CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Environment variable 'COSMICWIKI_PAYMENTS_CONNECTION' is not configured.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder.UseOracle(connectionString);

        return new PaymentDbContext(optionsBuilder.Options);
    }
}
