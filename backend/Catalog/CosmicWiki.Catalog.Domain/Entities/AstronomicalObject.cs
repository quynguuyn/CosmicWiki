using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Catalog.Domain.Entities;

public sealed class AstronomicalObject : Entity<Guid>
{
    private AstronomicalObject(Guid id)
        : base(id)
    {
    }

    public static AstronomicalObject Create()
    {
        return new AstronomicalObject(Guid.NewGuid());
    }

    public static AstronomicalObject Create(Guid id)
    {
        return new AstronomicalObject(id);
    }
}