using CosmicWiki.Catalog.Application.Research;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public interface IAstronomicalResearchImageEnricher
{
    Task<(string? MainUrl, bool Changed)> EnsureSurveyImagesAsync(
        Guid astronomicalObjectId,
        AstronomicalResearchCandidate candidate,
        CancellationToken cancellationToken);
}
