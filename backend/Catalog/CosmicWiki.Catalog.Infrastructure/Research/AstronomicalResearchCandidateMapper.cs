using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Domain.Entities;
using CosmicWiki.Catalog.Domain.Enums;
using CosmicWiki.Catalog.Domain.ValueObjects;

namespace CosmicWiki.Catalog.Infrastructure.Research;

internal static class AstronomicalResearchCandidateMapper
{
    public static AstronomicalObject CreateAstronomicalObject(
        Guid id,
        AstronomicalResearchCandidate candidate) =>
        AstronomicalObject.Create(
            id,
            candidate.Name,
            candidate.Kind,
            candidate.ObjectType,
            candidate.Catalog,
            candidate.CatalogValue,
            candidate.RightAscensionDegrees,
            candidate.DeclinationDegrees);

    public static Star CreateStar(
        Guid id,
        AstronomicalResearchCandidate candidate)
    {
        var star = Star.Create(id, candidate.Name);
        star.AddCatalogIdentifier(
            CatalogIdentifier.Create(candidate.Catalog, candidate.CatalogValue));
        star.AddObjectType(
            AstronomicalObjectType.Create(
                candidate.ObjectType,
                candidate.ObjectType,
                "Imported by the scheduled astronomical research job."));
        if (candidate.RightAscensionDegrees.HasValue &&
            candidate.DeclinationDegrees.HasValue)
        {
            star.AddCoordinate(
                AstronomicalCoordinate.Create(
                    candidate.RightAscensionDegrees.Value,
                    candidate.DeclinationDegrees.Value,
                    CoordinateSystem.ICRS,
                    2000.0));
        }

        if (candidate.ParallaxMas.HasValue)
            star.AddParallax(ParallaxMeasurement.Create(candidate.ParallaxMas.Value));

        return star;
    }
}
