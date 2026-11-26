namespace Etl.Core;

/// <summary>Inclusive date range a run should extract.</summary>
public readonly record struct ExtractionWindow(DateOnly From, DateOnly To);

public interface IExtractor<out TRaw>
{
    IAsyncEnumerable<TRaw> ExtractAsync(ExtractionWindow window, CancellationToken ct = default);
}

public interface ITransformer<in TRaw, TClean>
{
    TransformResult<TClean> Transform(TRaw raw);
}

public interface ILoader<in T>
{
    /// <summary>Loads a batch and returns how many rows were written. Must not keep a reference to <paramref name="batch"/>.</summary>
    Task<int> LoadAsync(IReadOnlyList<T> batch, CancellationToken ct = default);
}

public readonly record struct TransformResult<T>(T? Value, string? Error)
{
    public bool IsValid => Error is null;
    public static TransformResult<T> Ok(T value) => new(value, null);
    public static TransformResult<T> Fail(string error) => new(default, error);
}

public readonly record struct RunStats(int Read, int Loaded, int Rejected);
