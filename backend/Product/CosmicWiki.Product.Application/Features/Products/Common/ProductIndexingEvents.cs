using CosmicWiki.Messaging;
using CosmicWiki.Messaging.Abstractions;
using CosmicWiki.Messaging.Contracts.Product;
using ProductEntity = CosmicWiki.Product.Domain.Entities.Product;

namespace CosmicWiki.Product.Application.Features.Products.Common;

internal static class ProductIndexingEvents
{
    public static void AddUpdated(
        IOutboxWriter outboxWriter,
        ProductEntity product)
    {
        outboxWriter.Add(
            new ProductUpdatedIntegrationEvent(
                Guid.NewGuid(),
                1,
                DateTime.UtcNow,
                product.Id,
                product.Id,
                product.SKU,
                product.Name,
                product.Slug,
                product.Description,
                product.Images.OrderBy(x => x.DisplayOrder)
                    .Select(x => x.Url)
                    .FirstOrDefault()),
            KafkaTopics.ProductUpdated);
    }
}
