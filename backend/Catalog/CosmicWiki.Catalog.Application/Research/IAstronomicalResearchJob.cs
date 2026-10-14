namespace CosmicWiki.Catalog.Application.Research;

public interface IAstronomicalResearchJob
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
