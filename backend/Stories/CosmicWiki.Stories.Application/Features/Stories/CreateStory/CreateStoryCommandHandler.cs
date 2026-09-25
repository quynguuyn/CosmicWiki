using CosmicWiki.Caching;
using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Domain.Entities;
using MediatR;

namespace CosmicWiki.Stories.Application.Features.Stories.CreateStory;

public sealed class CreateStoryCommandHandler : IRequestHandler<CreateStoryCommand, Guid>
{
    private readonly IStoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStoryCache _cache;

    public CreateStoryCommandHandler(
        IStoryRepository repository,
        IUnitOfWork unitOfWork,
        IStoryCache cache)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateStoryCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var slug = request.Slug.Trim();
        if (await _repository.ExistsByCodeAsync(code, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Story code '{code}' already exists.");
        if (await _repository.ExistsBySlugAsync(slug, cancellationToken: cancellationToken))
            throw new InvalidOperationException($"Story slug '{slug}' already exists.");

        var story = Story.Create(
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
        await _repository.AddAsync(story, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync(story.Id, cancellationToken: cancellationToken);
        return story.Id;
    }
}
