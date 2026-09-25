using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.GetCategories;

public sealed record GetStoryCategoriesQuery : IRequest<IReadOnlyCollection<string>>;
