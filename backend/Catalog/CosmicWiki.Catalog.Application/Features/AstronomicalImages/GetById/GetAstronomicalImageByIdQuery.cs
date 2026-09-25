using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.GetById;

public sealed record GetAstronomicalImageByIdQuery(
    Guid AstronomicalObjectId,
    Guid ImageId
) : IRequest<AstronomicalImageDto?>;

public sealed record AstronomicalImageDto(
    Guid Id,
    Guid AstronomicalObjectId,
    string Url,
    IReadOnlyCollection<string> Urls);