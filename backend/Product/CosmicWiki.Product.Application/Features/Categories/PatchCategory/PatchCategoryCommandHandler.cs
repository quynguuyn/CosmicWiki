using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.PatchCategory;

public sealed class PatchCategoryCommandHandler
    : IRequestHandler<PatchCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryCache _categoryCache;

    public PatchCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICategoryCache categoryCache)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _categoryCache = categoryCache;
    }

    public async Task Handle(
        PatchCategoryCommand request,
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

        var name =
            request.Name ?? category.Name;

        var slug =
            request.Slug ?? category.Slug;

        var description =
            request.Description ?? category.Description;

        if (!string.Equals(
                name,
                category.Name,
                StringComparison.Ordinal))
        {
            if (await _categoryRepository.ExistsByNameAsync(
                    name,
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Category name '{name}' already exists.");
            }
        }

        if (!string.Equals(
                slug,
                category.Slug,
                StringComparison.Ordinal))
        {
            if (await _categoryRepository.ExistsBySlugAsync(
                    slug,
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Category slug '{slug}' already exists.");
            }
        }

        category.Update(
            name,
            slug,
            description);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _categoryCache.InvalidateAsync(request.Id, cancellationToken);
    }
}