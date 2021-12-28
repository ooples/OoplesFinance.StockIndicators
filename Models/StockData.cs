using OoplesFinance.StockIndicators.Enums;
using OoplesFinance.StockIndicators.Interfaces;
using static OoplesFinance.StockIndicators.Enums.SignalsClass;

namespace OoplesFinance.StockIndicators.Models
{
    [Serializable]
    public class StockData : IStockData
    {
        public InputName InputName { get; set; }
        public IndicatorName IndicatorName { get; set; }
        public List<decimal> InputValues { get; set; }
        public List<decimal> OpenPrices { get; set; }
        public List<decimal> HighPrices { get; set; }
        public List<decimal> LowPrices { get; set; }
        public List<decimal> ClosePrices { get; set; }
        public List<decimal> Volumes { get; set; }
        public List<decimal> CustomValuesList { get; set; }
        public Dictionary<string, List<decimal>> OutputValues { get; set; }
        public List<Signal> SignalsList { get; set; }
        public int Count { get; set; }

        public StockData(IEnumerable<decimal> openPrices, IEnumerable<decimal> highPrices, IEnumerable<decimal> lowPrices, IEnumerable<decimal> closePrices, 
            IEnumerable<decimal> volumes)
        {
            OpenPrices = new List<decimal>(openPrices);
            HighPrices = new List<decimal>(highPrices);
            LowPrices = new List<decimal>(lowPrices);
            ClosePrices = new List<decimal>(closePrices);
            Volumes = new List<decimal>(volumes);
            InputValues = new List<decimal>(closePrices);
            CustomValuesList = new List<decimal>();
            OutputValues = new Dictionary<string, List<decimal>>();
            SignalsList = new List<Signal>();
            InputName = InputName.Close;
            IndicatorName = IndicatorName.None;
            Count = (OpenPrices.Count + HighPrices.Count + LowPrices.Count + ClosePrices.Count + Volumes.Count) / 5 == ClosePrices.Count ? ClosePrices.Count : 0;
        }
    }
}
