namespace CosmicWiki.Stories.Application.Features.NasaApod;

public sealed record NasaApodSourceStatus(
    bool IsReachable,
    string? Message,
    DateTime CheckedAtUtc);

public sealed record NasaApodEntry(
    DateOnly Date,
    string Title,
    string Explanation,
    string? ImageUrl,
    string? Copyright);

public interface INasaApodProvider
{
    Task<NasaApodSourceStatus> GetStatusAsync(
        CancellationToken cancellationToken = default);

    Task<NasaApodEntry> GetTodayAsync(
        CancellationToken cancellationToken = default);
}

public interface INasaApodStoryJobScheduler
{
    string Enqueue();
}
