namespace CosmicWiki.SharedKernel.Abstractions;

public abstract class Entity<TId>
{
    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id!);
    }
}