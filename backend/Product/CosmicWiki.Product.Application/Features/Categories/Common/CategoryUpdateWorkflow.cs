using CosmicWiki.Caching;
using CosmicWiki.Product.Application.Abstractions.Persistence;
using CosmicWiki.Product.Domain.Entities;

namespace CosmicWiki.Product.Application.Features.Categories.Common;

internal static class CategoryUpdateWorkflow
{
    public static async Task ApplyAsync(
        Category category,
        string name,
        string slug,
        string? description,
        bool checkUnchangedValues,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ICategoryCache categoryCache,
        CancellationToken cancellationToken)
    {
        if ((checkUnchangedValues ||
             !string.Equals(name, category.Name, StringComparison.Ordinal)) &&
            await categoryRepository.ExistsByNameAsync(name, cancellationToken) &&
            !string.Equals(category.Name, name, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Category name '{name}' already exists.");
        }

        if ((checkUnchangedValues ||
             !string.Equals(slug, category.Slug, StringComparison.Ordinal)) &&
            await categoryRepository.ExistsBySlugAsync(slug, cancellationToken) &&
            !string.Equals(category.Slug, slug, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Category slug '{slug}' already exists.");
        }

        category.Update(name, slug, description);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await categoryCache.InvalidateAsync(category.Id, cancellationToken);
    }
}
