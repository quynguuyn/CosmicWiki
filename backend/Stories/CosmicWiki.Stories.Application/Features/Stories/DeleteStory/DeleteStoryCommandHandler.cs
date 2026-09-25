using CosmicWiki.Caching;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.DeleteStory;

public sealed class DeleteStoryCommandHandler : IRequestHandler<DeleteStoryCommand>
{
    private readonly IStoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStoryCache _cache;

    public DeleteStoryCommandHandler(
        IStoryRepository repository,
        IUnitOfWork unitOfWork,
        IStoryCache cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(DeleteStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Story '{request.Id}' was not found.");
        var previousSlug = story.Slug;
        _repository.Remove(story);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync(story.Id, previousSlug, cancellationToken);
    }
}
