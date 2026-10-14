namespace CosmicWiki.Messaging;

public static class KafkaTopics
{
    public const string CatalogStarCreated =
        "cosmicwiki.catalog.star-created";

    public const string CatalogAstronomicalObjectCreated =
        "cosmicwiki.catalog.object-created";

    public const string ProductCreated =
        "cosmicwiki.product.created";

    public const string ProductUpdated =
        "cosmicwiki.product.updated";

    public const string ProductDeleted =
        "cosmicwiki.product.deleted";

    public const string PaymentStatusChanged =
        "cosmicwiki.payments.status-changed";

    public const string PaymentStatusChangedDeadLetter =
        "cosmicwiki.payments.status-changed.dlq";
}
