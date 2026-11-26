using Etl.Core;
using Microsoft.Extensions.Logging;

namespace Etl.Infrastructure;

/// <summary>Placeholder loader for step 1. Replace with a PostgresLoader (Npgsql COPY) in step 2.</summary>
public sealed class LoggingLoader<T>(ILogger<LoggingLoader<T>> logger) : ILoader<T>
{
    public Task<int> LoadAsync(IReadOnlyList<T> batch, CancellationToken ct = default)
    {
        logger.LogInformation("Would load {Count} rows. First: {First}", batch.Count, batch[0]);
        return Task.FromResult(batch.Count);
    }
}
