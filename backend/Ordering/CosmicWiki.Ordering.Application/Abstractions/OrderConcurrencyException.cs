namespace CosmicWiki.Ordering.Application.Abstractions;

public sealed class OrderConcurrencyException : Exception
{
    public OrderConcurrencyException(Exception innerException)
        : base("The order changed concurrently.", innerException)
    {
    }
}
