//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

global using OoplesFinance.StockIndicators.Interfaces;

namespace OoplesFinance.StockIndicators.Models;

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
    public List<DateTime> Dates { get; set; }
    public List<TickerData> TickerDataList { get; set; }
    public List<decimal> CustomValuesList { get; set; }
    public Dictionary<string, List<decimal>> OutputValues { get; set; }
    public List<Signal> SignalsList { get; set; }
    public int Count { get; set; }

    /// <summary>
    /// Initializes the StockData Class using prebuilt lists of price information
    /// </summary>
    /// <param name="openPrices"></param>
    /// <param name="highPrices"></param>
    /// <param name="lowPrices"></param>
    /// <param name="closePrices"></param>
    /// <param name="volumes"></param>
    /// <param name="dates"></param>
    public StockData(IEnumerable<decimal> openPrices, IEnumerable<decimal> highPrices, IEnumerable<decimal> lowPrices, IEnumerable<decimal> closePrices,
        IEnumerable<decimal> volumes, IEnumerable<DateTime> dates, InputName inputName = InputName.Close)
    {
        OpenPrices = new List<decimal>(openPrices);
        HighPrices = new List<decimal>(highPrices);
        LowPrices = new List<decimal>(lowPrices);
        ClosePrices = new List<decimal>(closePrices);
        Volumes = new List<decimal>(volumes);
        Dates = new List<DateTime>(dates);
        InputValues = new List<decimal>(closePrices);
        CustomValuesList = new List<decimal>();
        OutputValues = new Dictionary<string, List<decimal>>();
        SignalsList = new List<Signal>();
        InputName = inputName;
        IndicatorName = IndicatorName.None;
        Count = (OpenPrices.Count + HighPrices.Count + LowPrices.Count + ClosePrices.Count + Volumes.Count + Dates.Count) / 6 == ClosePrices.Count ? ClosePrices.Count : 0;

        TickerDataList = new List<TickerData>();
        for (int i = 0; i < Count; i++)
        {
            var open = OpenPrices[i];
            var high = HighPrices[i];
            var low = LowPrices[i];
            var close = ClosePrices[i];
            var volume = Volumes[i];
            var date = Dates[i];

            var ticker = new TickerData()
            {
                Date = date,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume
            };
            TickerDataList.Add(ticker);
        }
    }

    /// <summary>
    /// Initializes the StockData Class using classic list of ticker information
    /// </summary>
    /// <param name="tickerDataList"></param>
    public StockData(IEnumerable<TickerData> tickerDataList, InputName inputName = InputName.Close)
    {
        OpenPrices = new List<decimal>();
        HighPrices = new List<decimal>();
        LowPrices = new List<decimal>();
        ClosePrices = new List<decimal>();
        Volumes = new List<decimal>();
        Dates = new List<DateTime>();
        CustomValuesList = new List<decimal>();
        OutputValues = new Dictionary<string, List<decimal>>();
        SignalsList = new List<Signal>();
        InputName = inputName;

        for (int i = 0; i < tickerDataList.Count(); i++)
        {
            var ticker = tickerDataList.ElementAt(i);

            var date = ticker.Date;
            Dates.Add(date);

            var open = ticker.Open;
            OpenPrices.AddRounded(open);

            var high = ticker.High;
            HighPrices.AddRounded(high);

            var low = ticker.Low;
            LowPrices.AddRounded(low);

            var close = ticker.Close;
            ClosePrices.AddRounded(close);

            var volume = ticker.Volume;
            Volumes.AddRounded(volume);
        }
        
        TickerDataList = tickerDataList.ToList();
        InputValues = new List<decimal>(ClosePrices);
        Count = (OpenPrices.Count + HighPrices.Count + LowPrices.Count + ClosePrices.Count + Volumes.Count + Dates.Count) / 6 == ClosePrices.Count ? ClosePrices.Count : 0;
    }
}