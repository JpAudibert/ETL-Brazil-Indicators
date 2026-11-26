namespace Etl.Core;

/// <summary>Row exactly as the BCB SGS API returns it (strings, dd/MM/yyyy dates).</summary>
public sealed record BcbRawPoint(int SeriesId, string Data, string Valor);

/// <summary>Clean, typed row ready for the database.</summary>
public sealed record EconomicIndicator(int SeriesId, DateOnly Date, decimal Value);
