using CosmicWiki.Caching;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.UpdateStory;

public sealed class UpdateStoryCommandHandler : IRequestHandler<UpdateStoryCommand>
{
    private readonly IStoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStoryCache _cache;

    public UpdateStoryCommandHandler(
        IStoryRepository repository,
        IUnitOfWork unitOfWork,
        IStoryCache cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(UpdateStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Story '{request.Id}' was not found.");
        var previousSlug = story.Slug;
        var code = request.Code.Trim();
        var slug = request.Slug.Trim();
        if (!string.Equals(story.Code, code, StringComparison.OrdinalIgnoreCase) &&
            await _repository.ExistsByCodeAsync(code, request.Id, cancellationToken))
            throw new InvalidOperationException($"Story code '{code}' already exists.");
        if (!string.Equals(story.Slug, slug, StringComparison.OrdinalIgnoreCase) &&
            await _repository.ExistsBySlugAsync(slug, request.Id, cancellationToken))
            throw new InvalidOperationException($"Story slug '{slug}' already exists.");

        story.Update(
            request.Category,
            request.Era,
            request.Code,
            request.Title,
            request.Slug,
            request.Description,
            request.Content,
            request.Author,
            request.ReadTimeMinutes,
            request.ImageUrl,
            request.ImageAlt,
            request.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync(story.Id, previousSlug, cancellationToken);
    }
}
