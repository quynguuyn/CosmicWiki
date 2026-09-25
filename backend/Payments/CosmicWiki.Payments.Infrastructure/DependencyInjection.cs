using CosmicWiki.Payments.Application.Abstractions;
using CosmicWiki.Payments.Infrastructure.Persistence;
using CosmicWiki.Payments.Infrastructure.Persistence.Repositories;
using CosmicWiki.Payments.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmicWiki.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentsDatabase")
            ?? throw new InvalidOperationException(
                "PaymentsDatabase connection string is not configured.");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseOracle(connectionString));
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentEventWriter, PaymentOutboxWriter>();
        services.AddHostedService<PaymentOutboxPublisher>();
        return services;
    }
}
