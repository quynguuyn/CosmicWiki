using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Categories.Common;
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

        await CategoryUpdateWorkflow.ApplyAsync(
            category,
            name,
            slug,
            description,
            checkUnchangedValues: false,
            _categoryRepository,
            _unitOfWork,
            _categoryCache,
            cancellationToken);
    }
}