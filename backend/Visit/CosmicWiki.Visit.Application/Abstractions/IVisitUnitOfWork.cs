namespace CosmicWiki.Visit.Application.Abstractions;
public interface IVisitUnitOfWork { Task<int> SaveChangesAsync(CancellationToken cancellationToken); }
