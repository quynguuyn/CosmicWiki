using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Update;

public sealed record UpdateAstronomicalImageCommand(
    Guid AstronomicalObjectId,
    Guid ImageId,
    string Url,
    IReadOnlyCollection<string>? Urls
) : IRequest;