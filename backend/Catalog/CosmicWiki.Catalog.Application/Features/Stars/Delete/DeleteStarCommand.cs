using MediatR;

namespace CosmicWiki.Catalog.Application.Features.Stars.Delete;

public sealed record DeleteStarCommand(
    Guid Id
) : IRequest;