namespace OoplesFinance.StockIndicators.Interfaces;

public interface IStockData
{
    InputName InputName { get; }
    IndicatorName IndicatorName { get; }
    List<decimal> InputValues { get; }
    List<decimal> OpenPrices { get; }
    List<decimal> HighPrices { get; }
    List<decimal> LowPrices { get; }
    List<decimal> ClosePrices { get; }
    List<decimal> Volumes { get; }
    List<decimal> CustomValuesList { get; }
    Dictionary<string, List<decimal>> OutputValues { get; }
    List<Signal> SignalsList { get; }
    int Count { get; }
}