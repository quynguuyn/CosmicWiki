using CosmicWiki.Catalog.Application.Research;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CosmicWiki.Catalog.Infrastructure.Research;

public sealed class SimBadAstronomicalResearchProvider : IAstronomicalResearchProvider
{
    private readonly HttpClient _httpClient;
    private readonly AstronomicalResearchOptions _options;
    private readonly ILogger<SimBadAstronomicalResearchProvider> _logger;

    public SimBadAstronomicalResearchProvider(
        HttpClient httpClient,
        IOptions<AstronomicalResearchOptions> options,
        ILogger<SimBadAstronomicalResearchProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AstronomicalResearchCandidate>> FindCandidatesAsync(
        CancellationToken cancellationToken = default)
    {
        var query = Uri.EscapeDataString(_options.Query);
        var endpoint = $"{_options.Endpoint}?request=doQuery&lang=adql&format=csv&query={query}";

        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (content.TrimStart().StartsWith('<'))
            throw new InvalidOperationException(
                "SIMBAD returned an error response instead of CSV data.");

        var lines = content.Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length < 2)
            return [];

        var candidates = new List<AstronomicalResearchCandidate>();

        foreach (var line in lines.Skip(1).Take(_options.PageSize))
        {
            var columns = ParseCsvLine(line);
            if (columns.Length < 2)
                continue;

            var name = columns[0].Trim().Trim('"');
            var objectType = columns[1].Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(objectType))
                continue;

            candidates.Add(
                new AstronomicalResearchCandidate(
                    name,
                    objectType,
                    "SIMBAD",
                    name));
        }

        _logger.LogInformation(
            "SIMBAD research returned {CandidateCount} astronomical candidates.",
            candidates.Count);

        return candidates;
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new System.Text.StringBuilder();
        var quoted = false;

        foreach (var character in line)
        {
            if (character == '"')
            {
                quoted = !quoted;
                continue;
            }

            if (character == ',' && !quoted)
            {
                values.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(character);
        }

        values.Add(value.ToString());
        return values.ToArray();
    }
}
