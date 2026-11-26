using System.Globalization;
using Etl.Core;

namespace Etl.Infrastructure.Bcb;

public sealed class BcbTransformer : ITransformer<BcbRawPoint, EconomicIndicator>
{
    public TransformResult<EconomicIndicator> Transform(BcbRawPoint raw)
    {
        DateOnly date;
        if (!DateOnly.TryParseExact(raw.Data, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date))
            return TransformResult<EconomicIndicator>.Fail($"Invalid date '{raw.Data}'");

        decimal value;
        if (!decimal.TryParse(raw.Valor, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return TransformResult<EconomicIndicator>.Fail($"Invalid value '{raw.Valor}'");

        return TransformResult<EconomicIndicator>.Ok(new EconomicIndicator(raw.SeriesId, date, value));
    }
}
