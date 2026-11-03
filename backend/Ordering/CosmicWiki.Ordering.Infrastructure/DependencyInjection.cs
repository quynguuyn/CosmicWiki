using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Ordering.Application.Abstractions;
using CosmicWiki.Ordering.Infrastructure.Catalog;
using CosmicWiki.Ordering.Infrastructure.Inventory;
using CosmicWiki.Ordering.Infrastructure.Messaging;
using CosmicWiki.Ordering.Infrastructure.Notifications;
using CosmicWiki.Ordering.Infrastructure.Payment;
using CosmicWiki.Ordering.Infrastructure.Persistence;
using CosmicWiki.Ordering.Infrastructure.Persistence.Outbox;
using CosmicWiki.Ordering.Infrastructure.Persistence.Repositories;
using CosmicWiki.Ordering.Infrastructure.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderingDatabase")
            ?? throw new InvalidOperationException(
                "OrderingDatabase connection string is not configured.");

        services.AddDbContext<OrderingDbContext>(options =>
            options.UseOracle(connectionString));
        services.AddScoped<OrderRepository>();
        services.AddScoped<IOrderRepository>(sp => sp.GetRequiredService<OrderRepository>());
        services.AddScoped<IOrderPaymentStatusUpdater>(sp => sp.GetRequiredService<OrderRepository>());
        services.AddScoped<IOrderPaymentIntentValidator>(sp => sp.GetRequiredService<OrderRepository>());
        services.Configure<ProductServiceOptions>(
            configuration.GetSection(ProductServiceOptions.SectionName));
        services.AddHttpClient<ProductServiceClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<ProductServiceOptions>>()
                .Value;
            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
                throw new InvalidOperationException("ProductService:BaseUrl must be an absolute URL.");
            client.BaseAddress = new Uri(baseUri, "internal/product/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddScoped<IProductSnapshotProvider>(sp =>
            sp.GetRequiredService<ProductServiceClient>());
        services.AddScoped<IInventoryReservationService>(sp =>
            sp.GetRequiredService<ProductServiceClient>());
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderNotificationRepository, OrderNotificationRepository>();
        services.AddHostedService<InventoryReservationReaper>();
        services.Configure<PaymentProviderOptions>(
            configuration.GetSection(PaymentProviderOptions.SectionName));
        services.AddHttpClient<StripePaymentProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<PaymentProviderOptions>>()
                .Value;
            client.BaseAddress = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddSingleton<FakePaymentProvider>();
        services.AddScoped<ConfiguredPaymentProvider>();
        services.AddScoped<IPaymentProvider>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<PaymentProviderOptions>>()
                .Value;
            return options.Mode.Trim().ToLowerInvariant() switch
            {
                "stripe" => serviceProvider.GetRequiredService<StripePaymentProvider>(),
                "fake" or "disabled" => serviceProvider.GetRequiredService<ConfiguredPaymentProvider>(),
                _ => throw new InvalidOperationException(
                    $"Unsupported payment provider mode '{options.Mode}'.")
            };
        });
        services.AddScoped<IOutboxWriter, OrderingOutboxWriter>();
        services.AddHostedService<OrderingOutboxPublisher>();
        services.AddHostedService<PaymentStatusChangedConsumer>();
        services.AddHostedService<OrderCreatedNotificationConsumer>();
        services.Configure<OrderLifecycleOptions>(
            configuration.GetSection(OrderLifecycleOptions.SectionName));
        services.AddSingleton<IOrderLifecycleSettings>(sp =>
            sp.GetRequiredService<IOptions<OrderLifecycleOptions>>().Value);
        services.AddHostedService<OrderLifecycleProcessor>();
        services.Configure<OrderEmailOptions>(
            configuration.GetSection(OrderEmailOptions.SectionName));
        services.AddSingleton<IOrderEmailSender, SmtpOrderEmailSender>();
        services.AddScoped<IOrderNotificationPublisher, OrderNotificationPublisher>();
        return services;
    }
}
