using CosmicWiki.Stories.Application.Abstractions.Persistence;

namespace CosmicWiki.Stories.Infrastructure.Persistence.Repositories;

public sealed class StoryUnitOfWork(StoriesDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
