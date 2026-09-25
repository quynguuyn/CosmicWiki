using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Update;

public sealed record UpdateAstronomicalImageRequest(
    string Url,
    IReadOnlyCollection<string>? Urls
) : IRequest;