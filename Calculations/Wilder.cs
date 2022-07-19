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
    /// Calculates the average true range.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAverageTrueRange(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, 
        int length = 14)
    {
        List<decimal> trList = new();
        List<Signal> signalsList = new();

        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);
        var emaList = GetMovingAverageList(stockData, maType, length, inputList);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;

            decimal currentTrueRange = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trList.AddRounded(currentTrueRange);
        }

        var atrList = GetMovingAverageList(stockData, maType, length, trList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList[i];
            decimal atr = atrList[i];
            decimal currentEma = emaList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;
            decimal prevEma = i >= 1 ? emaList[i - 1] : 0;
            decimal prevAtr = i >= 1 ? atrList[i - 1] : 0;
            decimal atrEma = CalculateEMA(atr, prevAtr, length);

            var signal = GetVolatilitySignal(currentValue - currentEma, prevValue - prevEma, atr, atrEma);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Atr", atrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = atrList;
        stockData.IndicatorName = IndicatorName.AverageTrueRange;

        return stockData;
    }

    /// <summary>
    /// Calculates the average index of the directional.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="maType">Type of the ma.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static StockData CalculateAverageDirectionalIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.WildersSmoothingMethod, int length = 14)
    {
        List<decimal> dmPlusList = new();
        List<decimal> dmMinusList = new();
        List<decimal> diPlusList = new();
        List<decimal> diMinusList = new();
        List<decimal> trList = new();
        List<decimal> diList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentHigh = highList[i];
            decimal currentLow = lowList[i];
            decimal prevHigh = i >= 1 ? highList[i - 1] : 0;
            decimal prevLow = i >= 1 ? lowList[i - 1] : 0;
            decimal prevClose = i >= 1 ? inputList[i - 1] : 0;
            decimal highDiff = currentHigh - prevHigh;
            decimal lowDiff = prevLow - currentLow;

            decimal dmPlus = highDiff > lowDiff ? Math.Max(highDiff, 0) : 0;
            dmPlusList.AddRounded(dmPlus);

            decimal dmMinus = highDiff < lowDiff ? Math.Max(lowDiff, 0) : 0;
            dmMinusList.AddRounded(dmMinus);

            decimal tr = CalculateTrueRange(currentHigh, currentLow, prevClose);
            trList.AddRounded(tr);
        }

        var dmPlus14List = GetMovingAverageList(stockData, maType, length, dmPlusList);
        var dmMinus14List = GetMovingAverageList(stockData, maType, length, dmMinusList);
        var tr14List = GetMovingAverageList(stockData, maType, length, trList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal dmPlus14 = dmPlus14List[i];
            decimal dmMinus14 = dmMinus14List[i];
            decimal trueRange14 = tr14List[i];

            decimal diPlus = trueRange14 != 0 ? MinOrMax(100 * dmPlus14 / trueRange14, 100, 0) : 0;
            diPlusList.AddRounded(diPlus);

            decimal diMinus = trueRange14 != 0 ? MinOrMax(100 * dmMinus14 / trueRange14, 100, 0) : 0;
            diMinusList.AddRounded(diMinus);

            decimal diDiff = Math.Abs(diPlus - diMinus);
            decimal diSum = diPlus + diMinus;

            decimal di = diSum != 0 ? MinOrMax(100 * diDiff / diSum, 100, 0) : 0;
            diList.AddRounded(di);
        }

        var adxList = GetMovingAverageList(stockData, maType, length, diList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal diPlus = diPlusList[i];
            decimal diMinus = diMinusList[i];
            decimal prevDiPlus = i >= 1 ? diPlusList[i - 1] : 0;
            decimal prevDiMinus = i >= 1 ? diMinusList[i - 1] : 0;

            var signal = GetCompareSignal(diPlus - diMinus, prevDiPlus - prevDiMinus);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "DiPlus", diPlusList },
            { "DiMinus", diMinusList },
            { "Adx", adxList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = adxList;
        stockData.IndicatorName = IndicatorName.AverageDirectionalIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Welles Wilder Volatility System
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    public static StockData CalculateWellesWilderVolatilitySystem(this StockData stockData,
        MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, int length1 = 63, int length2 = 21, decimal factor = 3)
    {
        List<decimal> vstopList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var atrList = CalculateAverageTrueRange(stockData, maType, length2).CustomValuesList;
        var emaList = GetMovingAverageList(stockData, maType, length1, inputList);
        var (highestList, lowestList) = GetMaxAndMinValuesList(inputList, length2);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentAtr = atrList[i];
            decimal currentEma = emaList[i];
            decimal highest = highestList[i];
            decimal lowest = lowestList[i];
            decimal currentValue = inputList[i];
            decimal prevValue = i >= 1 ? inputList[i - 1] : 0;

            decimal prevVStop = vstopList.LastOrDefault();
            decimal sic = currentValue > currentEma ? highest : lowest;
            decimal vstop = currentValue > currentEma ? sic - (factor * currentAtr) : sic + (factor * currentAtr);
            vstopList.AddRounded(vstop);

            var signal = GetCompareSignal(currentValue - vstop, prevValue - prevVStop);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Wwvs", vstopList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = vstopList;
        stockData.IndicatorName = IndicatorName.WellesWilderVolatilitySystem;

        return stockData;
    }
}