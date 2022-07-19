//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the typical price.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateTypicalPrice(this StockData stockData)
    {
        List<decimal> tpList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentClose = inputList[i];
            decimal prevTypicalPrice1 = i >= 1 ? tpList[i - 1] : 0;
            decimal prevTypicalPrice2 = i >= 2 ? tpList[i - 2] : 0;

            decimal typicalPrice = (currentHigh + currentLow + currentClose) / 3;
            tpList.AddRounded(typicalPrice);

            Signal signal = GetCompareSignal(typicalPrice - prevTypicalPrice1, prevTypicalPrice1 - prevTypicalPrice2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Tp", tpList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tpList;
        stockData.IndicatorName = IndicatorName.TypicalPrice;

        return stockData;
    }

    /// <summary>
    /// Calculates the median price.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateMedianPrice(this StockData stockData)
    {
        List<decimal> medianPriceList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentLow = lowList[i];
            decimal currentHigh = highList[i];
            decimal prevMedianPrice1 = i >= 1 ? medianPriceList[i - 1] : 0;
            decimal prevMedianPrice2 = i >= 2 ? medianPriceList[i - 2] : 0;

            decimal medianPrice = (currentHigh + currentLow) / 2;
            medianPriceList.AddRounded(medianPrice);

            Signal signal = GetCompareSignal(medianPrice - prevMedianPrice1, prevMedianPrice1 - prevMedianPrice2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "MedianPrice", medianPriceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = medianPriceList;
        stockData.IndicatorName = IndicatorName.MedianPrice;

        return stockData;
    }

    /// <summary>
    /// Calculates the full typical price.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateFullTypicalPrice(this StockData stockData)
    {
        List<decimal> fullTpList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentClose = inputList[i];
            decimal currentOpen = openList[i];
            decimal prevTypicalPrice1 = i >= 1 ? fullTpList[i - 1] : 0;
            decimal prevTypicalPrice2 = i >= 2 ? fullTpList[i - 2] : 0;

            decimal typicalPrice = (currentHigh + currentLow + currentClose + currentOpen) / 4;
            fullTpList.AddRounded(typicalPrice);

            Signal signal = GetCompareSignal(typicalPrice - prevTypicalPrice1, prevTypicalPrice1 - prevTypicalPrice2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FullTp", fullTpList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = fullTpList;
        stockData.IndicatorName = IndicatorName.FullTypicalPrice;

        return stockData;
    }

    /// <summary>
    /// Calculates the weighted close.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateWeightedClose(this StockData stockData)
    {
        List<decimal> weightedCloseList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal currentClose = inputList[i];
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;

            decimal prevWeightedClose = weightedCloseList.LastOrDefault();
            decimal weightedClose = (currentHigh + currentLow + (currentClose * 2)) / 4;
            weightedCloseList.AddRounded(weightedClose);

            Signal signal = GetCompareSignal(currentClose - weightedClose, prevClose - prevWeightedClose);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "WeightedClose", weightedCloseList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = weightedCloseList;
        stockData.IndicatorName = IndicatorName.WeightedClose;

        return stockData;
    }

    /// <summary>
    /// Calculates the Midpoint
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMidpoint(this StockData stockData, int length = 14)
    {
        List<decimal> midpointList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];

            decimal prevMidPoint = midpointList.LastOrDefault();
            decimal midpoint = (highest + lowest) / 2;
            midpointList.AddRounded(midpoint);

            var signal = GetCompareSignal(currentValue - midpoint, prevValue - prevMidPoint);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "HCLC2", midpointList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = midpointList;
        stockData.IndicatorName = IndicatorName.Midpoint;

        return stockData;
    }

    /// <summary>
    /// Calculates the Midprice
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateMidprice(this StockData stockData, int length = 14)
    {
        List<decimal> midpriceList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var (highestList, lowestList) = GetMaxAndMinValuesList(highList, lowList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];

            decimal prevMidPrice = midpriceList.LastOrDefault();
            decimal midPrice = (highest + lowest) / 2;
            midpriceList.AddRounded(midPrice);

            var signal = GetCompareSignal(currentValue - midPrice, prevValue - prevMidPrice);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "HHLL2", midpriceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = midpriceList;
        stockData.IndicatorName = IndicatorName.Midprice;

        return stockData;
    }
}
