using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CosmicWiki.Stories.Infrastructure.Persistence.Repositories;

public sealed class StoryRepository(StoriesDbContext dbContext) : IStoryRepository
{
    public Task<Story?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Stories.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Story?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Stories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Story?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        dbContext.Stories.AsNoTracking().SingleOrDefaultAsync(
            x => x.Slug.ToUpper() == slug.Trim().ToUpper(), cancellationToken);

    public async Task<IReadOnlyCollection<Story>> GetPageAsync(
        string? search,
        string? category,
        int skip,
        int take,
        string sort,
        CancellationToken cancellationToken = default)
    {
        var query = Filter(search, category);
        query = sort switch
        {
            "oldest" => query.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id),
            "title" => query.OrderBy(x => x.Title).ThenBy(x => x.Id),
            "duration" => query.OrderByDescending(x => x.ReadTimeMinutes).ThenBy(x => x.Id),
            "popular" => query.OrderBy(x => x.ReadTimeMinutes).ThenBy(x => x.Id),
            _ => query.OrderByDescending(x => x.CreatedAtUtc).ThenBy(x => x.Id)
        };
        return await query.AsNoTracking().Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken = default) =>
        Filter(search, category).CountAsync(cancellationToken);

    public async Task<IReadOnlyCollection<string>> GetCategoriesAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Stories.AsNoTracking()
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByCodeAsync(
        string code,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        dbContext.Stories.AnyAsync(
            x => x.Code.ToUpper() == code.Trim().ToUpper() &&
                 (!excludingId.HasValue || x.Id != excludingId.Value),
            cancellationToken);

    public Task<bool> ExistsBySlugAsync(
        string slug,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default) =>
        dbContext.Stories.AnyAsync(
            x => x.Slug.ToUpper() == slug.Trim().ToUpper() &&
                 (!excludingId.HasValue || x.Id != excludingId.Value),
            cancellationToken);

    public Task AddAsync(Story story, CancellationToken cancellationToken = default) =>
        dbContext.Stories.AddAsync(story, cancellationToken).AsTask();

    public void Remove(Story story) => dbContext.Stories.Remove(story);

    private IQueryable<Story> Filter(string? search, string? category)
    {
        var query = dbContext.Stories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim().ToUpper();
            query = query.Where(x => x.Category.ToUpper() == normalizedCategory);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToUpper();
            query = query.Where(x =>
                x.Title.ToUpper().Contains(normalizedSearch) ||
                x.Description.ToUpper().Contains(normalizedSearch) ||
                x.Content.ToUpper().Contains(normalizedSearch) ||
                x.Author.ToUpper().Contains(normalizedSearch) ||
                x.Era.ToUpper().Contains(normalizedSearch) ||
                x.Code.ToUpper().Contains(normalizedSearch));
        }
        return query;
    }
}
