using MediatR;

namespace CosmicWiki.Catalog.Application.Features.AstronomicalImages.Delete;

public sealed record DeleteAstronomicalImageCommand(
    Guid AstronomicalObjectId,
    Guid ImageId
) : IRequest;