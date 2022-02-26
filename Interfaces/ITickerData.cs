namespace OoplesFinance.StockIndicators.Interfaces;

public interface ITickerData
{
    public DateTime Date { get; }
    public decimal Open { get; }
    public decimal High { get; }
    public decimal Low { get; }
    public decimal Close { get; }
    public decimal Volume { get; }
}
