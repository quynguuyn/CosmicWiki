namespace CosmicWiki.SharedKernel.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}