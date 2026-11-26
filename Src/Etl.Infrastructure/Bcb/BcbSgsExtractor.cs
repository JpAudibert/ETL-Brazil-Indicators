using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Etl.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Etl.Infrastructure.Bcb;

public sealed class BcbSgsExtractor(
    HttpClient http,
    IOptions<BcbOptions> options,
    ILogger<BcbSgsExtractor> logger) : IExtractor<BcbRawPoint>
{
    private sealed record Dto(
        [property: JsonPropertyName("data")] string Data,
        [property: JsonPropertyName("valor")] string Valor);

    public async IAsyncEnumerable<BcbRawPoint> ExtractAsync(
        ExtractionWindow window,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        BcbOptions opts = options.Value;

        foreach (int seriesId in opts.SeriesIds)
        foreach (ExtractionWindow chunk in Chunk(window, opts.ChunkYears))
        {
            string url = $"dados/serie/bcdata.sgs.{seriesId}/dados?formato=json" +
                      $"&dataInicial={Fmt(chunk.From)}&dataFinal={Fmt(chunk.To)}";

            logger.LogInformation("BCB series {Series}: {From} -> {To}", seriesId, chunk.From, chunk.To);

            using HttpResponseMessage response = await http.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync(ct);

            await SaveRawAsync(opts.RawDirectory, seriesId, chunk, json, ct);

            List<Dto> points = JsonSerializer.Deserialize<List<Dto>>(json) ?? [];
            foreach (Dto p in points)
                yield return new BcbRawPoint(seriesId, p.Data, p.Valor);
        }
    }

    private static string Fmt(DateOnly d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static IEnumerable<ExtractionWindow> Chunk(ExtractionWindow w, int years)
    {
        DateOnly start = w.From;
        while (start <= w.To)
        {
            DateOnly end = start.AddYears(years).AddDays(-1);
            if (end > w.To) end = w.To;
            yield return new ExtractionWindow(start, end);
            start = end.AddDays(1);
        }
    }

    private static async Task SaveRawAsync(string root, int seriesId, ExtractionWindow w, string json, CancellationToken ct)
    {
        string dir = Path.Combine(root, "bcb", seriesId.ToString(CultureInfo.InvariantCulture));
        Directory.CreateDirectory(dir);
        string file = Path.Combine(dir, $"{w.From:yyyyMMdd}_{w.To:yyyyMMdd}.json");
        await File.WriteAllTextAsync(file, json, ct);
    }
}
