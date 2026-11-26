namespace Etl.Infrastructure.Bcb;

public sealed class BcbOptions
{
    /// <summary>SGS series codes: 1 = USD (sell), 432 = Selic target, 433 = IPCA monthly.</summary>
    public int[] SeriesIds { get; set; } = [1, 432, 433];

    /// <summary>Daily series reject very long windows, so requests are split into chunks of this many years.</summary>
    public int ChunkYears { get; set; } = 5;

    /// <summary>Raw JSON responses are saved here before any transformation.</summary>
    public string RawDirectory { get; set; } = "raw";
}
