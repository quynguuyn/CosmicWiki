using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Domain.Entities;
using MediatR;

namespace CosmicWiki.Product.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryCache _categoryCache;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICategoryCache categoryCache)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _categoryCache = categoryCache;
    }

    public async Task<Guid> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (await _categoryRepository.ExistsByNameAsync(
                request.Name,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Category name '{request.Name}' already exists.");
        }

        if (await _categoryRepository.ExistsBySlugAsync(
                request.Slug,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Category slug '{request.Slug}' already exists.");
        }

        var category = Category.Create(
            request.Name,
            request.Slug,
            request.Description);

        await _categoryRepository.AddAsync(
            category,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _categoryCache.InvalidateAsync(category.Id, cancellationToken);

        return category.Id;
    }
}