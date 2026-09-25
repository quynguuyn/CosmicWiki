using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryCache _categoryCache;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICategoryCache categoryCache)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _categoryCache = categoryCache;
    }

    public async Task Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category =
            await _categoryRepository.GetByIdForUpdateAsync(
                request.Id,
                cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Category '{request.Id}' was not found.");
        }

        _categoryRepository.Remove(category);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _categoryCache.InvalidateAsync(request.Id, cancellationToken);
    }
}