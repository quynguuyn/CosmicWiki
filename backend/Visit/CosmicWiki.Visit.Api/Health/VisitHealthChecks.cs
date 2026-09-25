using CosmicWiki.Visit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CosmicWiki.Visit.Api.Health;

public sealed class VisitDatabaseHealthCheck(
    IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<VisitDbContext>();
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Visit database is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Visit database is unavailable.",
                exception);
        }
    }
}
