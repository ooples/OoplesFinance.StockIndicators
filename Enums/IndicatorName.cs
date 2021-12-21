using OoplesFinance.StockIndicators.Helpers;

namespace OoplesFinance.StockIndicators.Enums
{
    public enum IndicatorName
    {
        None,
        [Category(IndicatorType.Volatility)]
        BollingerBands,
        [Category(IndicatorType.Trend)]
        ExponentialMovingAverage,
        [Category(IndicatorType.Trend)]
        FullTypicalPrice,
        [Category(IndicatorType.Trend)]
        MedianPrice,
        [Category(IndicatorType.Momentum)]
        MovingAverageConvergenceDivergence,
        [Category(IndicatorType.Momentum)]
        RelativeStrengthIndex,
        [Category(IndicatorType.Trend)]
        SimpleMovingAverage,
        [Category(IndicatorType.Volatility)]
        StandardDeviationVolatility,
        [Category(IndicatorType.Trend)]
        TypicalPrice,
        [Category(IndicatorType.Trend)]
        WeightedClose,
        [Category(IndicatorType.Trend)]
        WeightedMovingAverage
    }
}

