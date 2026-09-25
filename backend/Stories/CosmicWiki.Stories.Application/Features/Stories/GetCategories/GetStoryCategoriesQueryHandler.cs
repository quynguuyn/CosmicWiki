using CosmicWiki.Stories.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.GetCategories;

public sealed class GetStoryCategoriesQueryHandler : IRequestHandler<GetStoryCategoriesQuery, IReadOnlyCollection<string>>
{
    private readonly IStoryRepository _repository;

    public GetStoryCategoriesQueryHandler(IStoryRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyCollection<string>> Handle(
        GetStoryCategoriesQuery request,
        CancellationToken cancellationToken) =>
        _repository.GetCategoriesAsync(cancellationToken);
}
