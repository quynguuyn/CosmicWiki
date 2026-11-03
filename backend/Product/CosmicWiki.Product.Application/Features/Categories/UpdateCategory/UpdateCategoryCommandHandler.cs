using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Application.Features.Categories.Common;
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

        await CategoryUpdateWorkflow.ApplyAsync(
            category,
            request.Name,
            request.Slug,
            request.Description,
            checkUnchangedValues: true,
            _categoryRepository,
            _unitOfWork,
            _categoryCache,
            cancellationToken);
    }
}