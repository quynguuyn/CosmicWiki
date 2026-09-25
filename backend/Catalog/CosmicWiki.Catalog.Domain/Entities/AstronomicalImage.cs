using CosmicWiki.SharedKernel.Abstractions;

namespace CosmicWiki.Catalog.Domain.Entities;

public sealed class AstronomicalImage : Entity<Guid>
{
    private readonly List<string> _urls = [];

    private AstronomicalImage(
        Guid id,
        Guid astronomicalObjectId,
        string url)
        : base(id)
    {
        AstronomicalObjectId = astronomicalObjectId;
        Url = url;
    }

    public Guid AstronomicalObjectId { get; private set; }

    public string Url { get; private set; }

    public IReadOnlyCollection<string> Urls =>
        _urls.AsReadOnly();

    public static AstronomicalImage Create(
        Guid astronomicalObjectId,
        string url,
        IEnumerable<string>? urls = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var image = new AstronomicalImage(
            Guid.NewGuid(),
            astronomicalObjectId,
            url.Trim());

        if (urls is not null)
        {
            image._urls.AddRange(
                urls
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct());
        }

        return image;
    }
    
    public void Update(
        string url,
        IEnumerable<string>? urls = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        Url = url.Trim();

        _urls.Clear();

        if (urls is not null)
        {
            _urls.AddRange(
                urls
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct());
        }
    }
    
    public void ChangeUrl(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        Url = url.Trim();
    }

    public void ReplaceUrls(IEnumerable<string> urls)
    {
        ArgumentNullException.ThrowIfNull(urls);

        _urls.Clear();

        _urls.AddRange(
            urls
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct());
    }
}