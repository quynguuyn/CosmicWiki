using CosmicWiki.Catalog.Application.Abstractions.Persistence;
using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Domain.Entities;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class AstronomicalResearchImageEnricher(
    IAstronomicalImageRepository imageRepository) : IAstronomicalResearchImageEnricher
{
    public async Task<(string? MainUrl, bool Changed)> EnsureSurveyImagesAsync(
        Guid astronomicalObjectId,
        AstronomicalResearchCandidate candidate,
        CancellationToken cancellationToken)
    {
        if (!candidate.RightAscensionDegrees.HasValue ||
            !candidate.DeclinationDegrees.HasValue)
            return (null, false);

        var rightAscension = candidate.RightAscensionDegrees.Value;
        var declination = candidate.DeclinationDegrees.Value;
        var mainUrl = BuildSurveyImageUrl("CDS/P/DSS2/color", rightAscension, declination);
        var alternateUrls = new[]
        {
            BuildSurveyImageUrl("CDS/P/DSS2/red", rightAscension, declination),
            BuildSurveyImageUrl("CDS/P/DSS2/blue", rightAscension, declination),
            BuildSurveyImageUrl("CDS/P/DSS2/IR", rightAscension, declination),
            BuildSurveyImageUrl("CDS/P/2MASS/color", rightAscension, declination),
            BuildSurveyImageUrl("CDS/P/AllWISE/color", rightAscension, declination)
        };

        var images = await imageRepository.GetByAstronomicalObjectIdAsync(
            astronomicalObjectId,
            cancellationToken);
        var image = images.FirstOrDefault();
        if (image is null)
        {
            await imageRepository.AddAsync(
                AstronomicalImage.Create(astronomicalObjectId, mainUrl, alternateUrls),
                cancellationToken);
            return (mainUrl, true);
        }

        if (image.Url != mainUrl ||
            !image.Urls.SequenceEqual(alternateUrls, StringComparer.Ordinal))
        {
            image.Update(mainUrl, alternateUrls);
            return (mainUrl, true);
        }

        return (mainUrl, false);
    }

    private static string BuildSurveyImageUrl(
        string survey,
        double rightAscensionDegrees,
        double declinationDegrees) =>
        FormattableString.Invariant(
            $"https://alasky.cds.unistra.fr/hips-image-services/hips2fits?hips={Uri.EscapeDataString(survey)}&ra={rightAscensionDegrees:R}&dec={declinationDegrees:R}&fov=0.12&width=800&height=600&format=jpg");
}
