using CosmicWiki.Stories.Application.Features.Stories.Common;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.GetStoryBySlug;

public sealed record GetStoryBySlugQuery(string Slug) : IRequest<StoryResponse?>;
