using CosmicWiki.Caching;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.PatchStory;

public sealed class PatchStoryCommandHandler : IRequestHandler<PatchStoryCommand>
{
    private readonly IStoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStoryCache _cache;

    public PatchStoryCommandHandler(
        IStoryRepository repository,
        IUnitOfWork unitOfWork,
        IStoryCache cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task Handle(PatchStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Story '{request.Id}' was not found.");
        var previousSlug = story.Slug;
        var code = request.Code?.Trim() ?? story.Code;
        var slug = request.Slug?.Trim() ?? story.Slug;
        if (!string.Equals(story.Code, code, StringComparison.OrdinalIgnoreCase) &&
            await _repository.ExistsByCodeAsync(code, request.Id, cancellationToken))
            throw new InvalidOperationException($"Story code '{code}' already exists.");
        if (!string.Equals(story.Slug, slug, StringComparison.OrdinalIgnoreCase) &&
            await _repository.ExistsBySlugAsync(slug, request.Id, cancellationToken))
            throw new InvalidOperationException($"Story slug '{slug}' already exists.");

        story.Update(
            request.Category ?? story.Category,
            request.Era ?? story.Era,
            code,
            request.Title ?? story.Title,
            slug,
            request.Description ?? story.Description,
            request.Content ?? story.Content,
            request.Author ?? story.Author,
            request.ReadTimeMinutes ?? story.ReadTimeMinutes,
            request.ImageUrl ?? story.ImageUrl,
            request.ImageAlt ?? story.ImageAlt,
            request.Status ?? story.Status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync(story.Id, previousSlug, cancellationToken);
    }
}
