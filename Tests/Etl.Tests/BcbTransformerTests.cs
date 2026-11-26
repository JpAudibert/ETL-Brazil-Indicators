using Etl.Core;
using Etl.Infrastructure.Bcb;

namespace Etl.Tests;

public class BcbTransformerTests
{
    private readonly BcbTransformer _sut = new();

    [Test]
    public async Task Valid_row_is_parsed()
    {
        // Arrange
        BcbRawPoint point = new(1, "02/01/2024", "4.8413");

        // Act
        TransformResult<EconomicIndicator> result = _sut.Transform(point);

        // Assert
        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.Value).IsNotNull();
        await Assert.That(result.Value!.Date).IsEqualTo(new DateOnly(2024, 1, 2));
        await Assert.That(result.Value.Value).IsEqualTo(4.8413m);
    }

    [Test]
    [Arguments("2024-01-02", "4.84")] // wrong date format
    [Arguments("31/02/2024", "4.84")] // impossible date
    [Arguments("02/01/2024", "abc")] // non-numeric value
    [Arguments("02/01/2024", "")] // empty value
    public async Task Invalid_rows_are_rejected_with_a_reason(string data, string valor)
    {
        // Arrange
        BcbRawPoint point = new(1, data, valor);

        // Act
        TransformResult<EconomicIndicator> result = _sut.Transform(point);

        // Assert
        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(string.IsNullOrWhiteSpace(result.Error)).IsFalse();
    }
}
