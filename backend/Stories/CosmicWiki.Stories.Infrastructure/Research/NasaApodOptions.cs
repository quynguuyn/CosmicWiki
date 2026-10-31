namespace CosmicWiki.Stories.Infrastructure.Research;

public sealed class NasaApodOptions
{
    public const string SectionName = "NasaApod";
    public bool Enabled { get; init; } = true;
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.nasa.gov/planetary/apod";
    public int RequestTimeoutSeconds { get; init; } = 15;
}
