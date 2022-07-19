//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

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
    List<DateTime> Dates { get; }
    List<decimal> CustomValuesList { get; }
    Dictionary<string, List<decimal>> OutputValues { get; }
    List<Signal> SignalsList { get; }
    int Count { get; }
}