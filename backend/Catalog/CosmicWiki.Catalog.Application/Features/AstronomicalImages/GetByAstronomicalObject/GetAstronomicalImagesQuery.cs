using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.GetByAstronomicalObject;

public sealed record GetAstronomicalImagesQuery(
    Guid AstronomicalObjectId)
    : IRequest<IReadOnlyCollection<AstronomicalImageDto>>;

public sealed record AstronomicalImageDto(
    Guid Id,
    Guid AstronomicalObjectId,
    string Url,
    IReadOnlyCollection<string> Urls);