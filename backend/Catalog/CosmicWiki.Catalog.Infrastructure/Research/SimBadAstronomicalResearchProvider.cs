using CosmicWiki.Catalog.Application.Research;
using CosmicWiki.Catalog.Domain.Enums;
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

    public async Task<AstronomicalResearchSourceStatus> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            var query = Uri.EscapeDataString("SELECT TOP 1 main_id FROM basic");
            var endpoint = $"{_options.Endpoint}?request=doQuery&lang=adql&format=csv&query={query}";
            using var response = await _httpClient.GetAsync(endpoint, timeout.Token);
            var content = await response.Content.ReadAsStringAsync(timeout.Token);
            var reachable = response.IsSuccessStatusCode && !content.TrimStart().StartsWith('<');
            return new AstronomicalResearchSourceStatus(
                reachable,
                reachable ? null : $"SIMBAD returned HTTP {(int)response.StatusCode}.",
                DateTime.UtcNow);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new AstronomicalResearchSourceStatus(false, "SIMBAD did not respond within 5 seconds.", DateTime.UtcNow);
        }
        catch (HttpRequestException exception)
        {
            return new AstronomicalResearchSourceStatus(false, exception.Message, DateTime.UtcNow);
        }
    }

    public async Task<AstronomicalResearchPage> FindCandidatesAsync(
        string? afterMainId,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        pageSize = Math.Min(pageSize, 500);
        var query = Uri.EscapeDataString(
            BuildQuery(afterMainId, pageSize + 1));
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
            return new AstronomicalResearchPage([], null, false);

        var candidates = new List<AstronomicalResearchCandidate>();
        var rows = lines.Skip(1).Take(pageSize + 1).ToArray();
        var pageRows = rows.Take(pageSize).ToArray();

        foreach (var line in pageRows)
        {
            var columns = ParseCsvLine(line);
            if (columns.Length < 6 ||
                string.IsNullOrWhiteSpace(columns[0]) ||
                string.IsNullOrWhiteSpace(columns[1]) ||
                string.IsNullOrWhiteSpace(columns[2]))
                throw new InvalidOperationException(
                    "SIMBAD returned a malformed row in the astronomical research page.");

            var name = columns[0].Trim().Trim('"');
            var kindCode = columns[1].Trim().Trim('"');
            var objectType = columns[2].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(objectType))
                throw new InvalidOperationException(
                    "SIMBAD returned an empty identifier or object type in the astronomical research page.");

            var rightAscension = ParseOptionalDouble(columns[3]);
            var declination = ParseOptionalDouble(columns[4]);
            var parallax = ParseOptionalDouble(columns[5]);
            if ((rightAscension is null) != (declination is null))
                throw new InvalidOperationException(
                    $"SIMBAD returned incomplete coordinates for '{name}'.");

            candidates.Add(
                new AstronomicalResearchCandidate(
                    name,
                    kindCode switch
                    {
                        "*" => AstronomicalObjectKind.Star,
                        "G" => AstronomicalObjectKind.Galaxy,
                        "Pl" => AstronomicalObjectKind.Planet,
                        _ => AstronomicalObjectKind.Other
                    },
                    objectType,
                    "SIMBAD",
                    name,
                    rightAscension,
                    declination,
                    parallax is >= 0 ? parallax : null));
        }

        _logger.LogInformation(
            "SIMBAD research returned {CandidateCount} candidates; more results: {HasMore}.",
            candidates.Count,
            rows.Length > pageSize);

        return new AstronomicalResearchPage(
            candidates,
            candidates.LastOrDefault()?.CatalogValue,
            rows.Length > pageSize);
    }

    private static string BuildQuery(string? afterMainId, int take)
    {
        var cursorFilter = string.IsNullOrWhiteSpace(afterMainId)
            ? string.Empty
            : $"AND main_id > '{afterMainId.Replace("'", "''", StringComparison.Ordinal)}' ";

        return $"SELECT TOP {take} main_id, otype, otype_txt, ra, dec, plx_value FROM basic " +
               $"WHERE otype IN ('*', 'G', 'Pl') {cursorFilter}" +
               "ORDER BY main_id ASC";
    }

    private static double? ParseOptionalDouble(string value)
    {
        value = value.Trim().Trim('"');
        return string.IsNullOrWhiteSpace(value) ||
               value.Equals("null", StringComparison.OrdinalIgnoreCase)
            ? null
            : double.Parse(
                value,
                System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var value = new System.Text.StringBuilder();
        var quoted = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                    continue;
                }

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

        if (quoted)
            throw new FormatException("SIMBAD returned a malformed CSV row.");

        values.Add(value.ToString());
        return values.ToArray();
    }
}
