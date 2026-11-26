using Microsoft.Extensions.Logging;

namespace Etl.Core;

/// <summary>
/// Streams Extract -> Transform -> Load in batches so memory stays flat.
/// Step 1 is sequential on purpose; swap the loop for Channels once the loop works end to end.
/// </summary>
public sealed class PipelineRunner<TRaw, TClean>(
    IExtractor<TRaw> extractor,
    ITransformer<TRaw, TClean> transformer,
    ILoader<TClean> loader,
    ILogger<PipelineRunner<TRaw, TClean>> logger)
{
    public async Task<RunStats> RunAsync(ExtractionWindow window, int batchSize = 5_000, CancellationToken ct = default)
    {
        int read = 0, loaded = 0, rejected = 0;
        List<TClean> batch = new(batchSize);

        await foreach (TRaw raw in extractor.ExtractAsync(window, ct))
        {
            read++;
            TransformResult<TClean> result = transformer.Transform(raw);

            if (!result.IsValid)
            {
                rejected++;
                logger.LogWarning("Rejected {Raw}: {Reason}", raw, result.Error);
                continue; // TODO: write to a dead-letter table/file
            }

            batch.Add(result.Value!);
            if (batch.Count >= batchSize)
            {
                loaded += await loader.LoadAsync(batch, ct);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
            loaded += await loader.LoadAsync(batch, ct);

        return new RunStats(read, loaded, rejected);
    }
}
