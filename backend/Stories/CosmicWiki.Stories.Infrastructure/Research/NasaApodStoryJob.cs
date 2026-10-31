using CosmicWiki.Stories.Application.Abstractions.Persistence;
using CosmicWiki.Stories.Domain.Entities;
using CosmicWiki.Stories.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Stories.Infrastructure.Research;

public sealed class NasaApodStoryJob(
    INasaApodProvider provider,
    IStoryRepository stories,
    IUnitOfWork unitOfWork,
    IBackgroundJobClient backgroundJobs,
    IOptions<NasaApodOptions> options)
{
    private readonly NasaApodOptions _options = options.Value;

    [Queue("stories")]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return;

        var status = await provider.GetStatusAsync(cancellationToken);
        if (!status.IsReachable)
        {
            backgroundJobs.Schedule<NasaApodStoryJob>(
                job => job.RunAsync(CancellationToken.None),
                TimeSpan.FromMinutes(5));
            throw new HttpRequestException(
                status.Message ?? "NASA APOD is unavailable.");
        }

        var apod = await provider.GetTodayAsync(cancellationToken);
        var code = $"NASA-APOD-{apod.Date:yyyyMMdd}";
        if (await stories.ExistsByCodeAsync(code, cancellationToken: cancellationToken))
            return;

        var author = string.IsNullOrWhiteSpace(apod.Copyright)
            ? "NASA Astronomy Picture of the Day"
            : $"NASA APOD · {apod.Copyright}";
        var explanation = apod.Explanation.Trim();
        var story = Story.Create(
            "NASA APOD",
            apod.Date.Year.ToString(),
            code,
            apod.Title,
            $"nasa-apod-{apod.Date:yyyy-MM-dd}",
            explanation,
            explanation,
            author,
            Math.Max(
                1,
                (int)Math.Ceiling(
                    explanation.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                    / 200d)),
            apod.ImageUrl,
            apod.Title,
            StoryStatus.Published);
        await stories.AddAsync(story, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
