using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Add;

public sealed record AddAstronomicalImageCommand(
    Guid AstronomicalObjectId,
    string Url,
    IReadOnlyCollection<string>? Urls
) : IRequest<Guid>;