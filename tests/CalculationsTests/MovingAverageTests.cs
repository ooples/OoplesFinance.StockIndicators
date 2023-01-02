namespace OoplesFinance.StockIndicators.Tests.Unit.CalculationsTests;

public sealed class MovingAverageTests : GlobalTestData
{
    [Fact]
    public void CalculateSimpleMovingAverage_ReturnsProperValues()
    {
        // Arrange
        var stockData = new StockData(StockTestData);
        var expectedResults = GetCsvData<double>("MovingAverage/Sma");

        // Act
        var results = stockData.CalculateSimpleMovingAverage().CustomValuesList;

        // Assert
        results.Should().NotBeNullOrEmpty();
        results.Should().BeEquivalentTo(expectedResults);
    }
}