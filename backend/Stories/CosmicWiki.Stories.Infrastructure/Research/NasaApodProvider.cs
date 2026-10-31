using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Stories.Infrastructure.Research;

public interface INasaApodProvider
{
    Task<NasaApodSourceStatus> GetStatusAsync(
        CancellationToken cancellationToken = default);

    Task<NasaApodEntry> GetTodayAsync(
        CancellationToken cancellationToken = default);
}

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

public sealed class NasaApodProvider(
    HttpClient httpClient,
    IOptions<NasaApodOptions> options) : INasaApodProvider
{
    private readonly NasaApodOptions _options = options.Value;
    private static readonly TimeZoneInfo NasaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Eastern Standard Time" : "America/New_York");

    public async Task<NasaApodSourceStatus> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new(
                false,
                "NASA APOD API key is not configured.",
                DateTime.UtcNow);
        }

        using var timeout =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            using var response = await httpClient.GetAsync(
                BuildRequestUri(),
                timeout.Token);
            return new(
                response.IsSuccessStatusCode,
                response.IsSuccessStatusCode
                    ? null
                    : $"NASA APOD returned HTTP {(int)response.StatusCode}.",
                DateTime.UtcNow);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "NASA APOD did not respond within 5 seconds.", DateTime.UtcNow);
        }
        catch (HttpRequestException exception)
        {
            return new(false, exception.Message, DateTime.UtcNow);
        }
    }

    public async Task<NasaApodEntry> GetTodayAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "NASA APOD API key is not configured.");
        }

        var entry = await httpClient.GetFromJsonAsync<NasaApodResponse>(
            BuildRequestUri(),
            cancellationToken)
            ?? throw new InvalidOperationException(
                "NASA APOD returned an empty response.");

        if (string.IsNullOrWhiteSpace(entry.date)
            || string.IsNullOrWhiteSpace(entry.title)
            || string.IsNullOrWhiteSpace(entry.explanation))
        {
            throw new InvalidOperationException(
                "NASA APOD returned incomplete content.");
        }

        return new(
            DateOnly.Parse(entry.date),
            entry.title,
            entry.explanation,
            entry.hdurl ?? entry.url,
            entry.copyright);
    }

    private string BuildRequestUri()
    {
        var nasaDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NasaTimeZone));
        return $"?api_key={Uri.EscapeDataString(_options.ApiKey)}&date={nasaDate:yyyy-MM-dd}";
    }

    private sealed record NasaApodResponse(
        string? date,
        string? title,
        string? explanation,
        string? url,
        string? hdurl,
        string? copyright);
}
