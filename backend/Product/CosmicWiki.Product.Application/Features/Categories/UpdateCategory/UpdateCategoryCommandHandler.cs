using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryCache _categoryCache;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICategoryCache categoryCache)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _categoryCache = categoryCache;
    }

    public async Task Handle(
        UpdateCategoryCommand request,
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

        if (await _categoryRepository.ExistsByNameAsync(
                request.Name,
                cancellationToken))
        {
            if (!string.Equals(
                    category.Name,
                    request.Name,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Category name '{request.Name}' already exists.");
            }
        }

        if (await _categoryRepository.ExistsBySlugAsync(
                request.Slug,
                cancellationToken))
        {
            if (!string.Equals(
                    category.Slug,
                    request.Slug,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Category slug '{request.Slug}' already exists.");
            }
        }

        category.Update(
            request.Name,
            request.Slug,
            request.Description);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _categoryCache.InvalidateAsync(request.Id, cancellationToken);
    }
}