using CosmicWiki.Stories.Application.Features.Stories.Common;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.GetStoryById;

public sealed record GetStoryByIdQuery(Guid Id) : IRequest<StoryResponse?>;
