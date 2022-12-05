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
    /// Calculates the Demark Range Expansion Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDemarkRangeExpansionIndex(this StockData stockData, int length = 5)
    {
        List<double> s2List = new();
        List<double> s1List = new();
        List<double> reiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double high = highList[i];
            double prevHigh2 = i >= 2 ? highList[i - 2] : 0;
            double prevHigh5 = i >= 5 ? highList[i - 5] : 0;
            double prevHigh6 = i >= 6 ? highList[i - 6] : 0;
            double low = lowList[i];
            double prevLow2 = i >= 2 ? lowList[i - 2] : 0;
            double prevLow5 = i >= 5 ? lowList[i - 5] : 0;
            double prevLow6 = i >= 6 ? lowList[i - 6] : 0;
            double prevClose7 = i >= 7 ? inputList[i - 7] : 0;
            double prevClose8 = i >= 8 ? inputList[i - 8] : 0;
            double prevRei1 = i >= 1 ? reiList[i - 1] : 0;
            double prevRei2 = i >= 2 ? reiList[i - 2] : 0;
            double n = (high >= prevLow5 || high >= prevLow6) && (low <= prevHigh5 || low <= prevHigh6) ? 0 : 1;
            double m = prevHigh2 >= prevClose8 && (prevLow2 <= prevClose7 || prevLow2 <= prevClose8) ? 0 : 1;
            double s = high - prevHigh2 + (low - prevLow2);

            double s1 = n * m * s;
            s1List.AddRounded(s1);

            double s2 = Math.Abs(s);
            s2List.AddRounded(s2);

            double s1Sum = s1List.TakeLastExt(length).Sum();
            double s2Sum = s2List.TakeLastExt(length).Sum();

            double rei = s2Sum != 0 ? s1Sum / s2Sum * 100 : 0;
            reiList.AddRounded(rei);

            var signal = GetRsiSignal(rei - prevRei1, prevRei1 - prevRei2, rei, prevRei1, 100, -100);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Drei", reiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = reiList;
        stockData.IndicatorName = IndicatorName.DemarkRangeExpansionIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Demark Pressure Ratio V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDemarkPressureRatioV1(this StockData stockData, int length = 13)
    {
        List<double> bpList = new();
        List<double> spList = new();
        List<double> pressureRatioList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double currentClose = inputList[i];
            double currentVolume = volumeList[i];
            double prevClose = i >= 1 ? inputList[i - 1] : 0;
            double prevPr1 = i >= 1 ? pressureRatioList[i - 1] : 0;
            double prevPr2 = i >= 2 ? pressureRatioList[i - 2] : 0;
            double gapup = prevClose != 0 ? (currentOpen - prevClose) / prevClose : 0;
            double gapdown = currentOpen != 0 ? (prevClose - currentOpen) / currentOpen : 0;

            double bp = gapup > 0.15m ? (currentHigh - prevClose + currentClose - currentLow) * currentVolume :
                currentClose > currentOpen ? (currentClose - currentOpen) * currentVolume : 0;
            bpList.AddRounded(bp);

            double sp = gapdown > 0.15m ? (prevClose - currentLow + currentHigh - currentClose) * currentVolume :
                currentClose < currentOpen ? (currentClose - currentOpen) * currentVolume : 0;
            spList.AddRounded(sp);

            double bpSum = bpList.TakeLastExt(length).Sum();
            double spSum = spList.TakeLastExt(length).Sum();

            double pressureRatio = bpSum - spSum != 0 ? MinOrMax(100 * bpSum / (bpSum - spSum), 100, 0) : 0;
            pressureRatioList.AddRounded(pressureRatio);

            var signal = GetRsiSignal(pressureRatio - prevPr1, prevPr1 - prevPr2, pressureRatio, prevPr1, 75, 25);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dpr", pressureRatioList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pressureRatioList;
        stockData.IndicatorName = IndicatorName.DemarkPressureRatioV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Demark Pressure Ratio V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDemarkPressureRatioV2(this StockData stockData, int length = 10)
    {
        List<double> bpList = new();
        List<double> spList = new();
        List<double> pressureRatioList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, volumeList) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentHigh = highList[i];
            double currentLow = lowList[i];
            double currentOpen = openList[i];
            double currentClose = inputList[i];
            double currentVolume = volumeList[i];
            double delta = currentClose - currentOpen;
            double trueRange = currentHigh - currentLow;
            double ratio = trueRange != 0 ? delta / trueRange : 0;
            double prevPr1 = i >= 1 ? pressureRatioList[i - 1] : 0;
            double prevPr2 = i >= 2 ? pressureRatioList[i - 2] : 0;

            double buyingPressure = delta > 0 ? ratio * currentVolume : 0;
            bpList.AddRounded(buyingPressure);

            double sellingPressure = delta < 0 ? ratio * currentVolume : 0;
            spList.AddRounded(sellingPressure);

            double bpSum = bpList.TakeLastExt(length).Sum();
            double spSum = spList.TakeLastExt(length).Sum();
            double denom = bpSum + Math.Abs(spSum);

            double pressureRatio = denom != 0 ? MinOrMax(100 * bpSum / denom, 100, 0) : 50;
            pressureRatioList.AddRounded(pressureRatio);

            var signal = GetRsiSignal(pressureRatio - prevPr1, prevPr1 - prevPr2, pressureRatio, prevPr1, 75, 25);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dpr", pressureRatioList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pressureRatioList;
        stockData.IndicatorName = IndicatorName.DemarkPressureRatioV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Demark Reversal Points
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateDemarkReversalPoints(this StockData stockData, int length1 = 9, int length2 = 4)
    {
        List<double> drpPriceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double uCount = 0, dCount = 0;
            for (int j = 0; j < length1; j++)
            {
                double value = i >= j ? inputList[i - j] : 0;
                double prevValue = i >= j + length2 ? inputList[i - (j + length2)] : 0;

                uCount += value > prevValue ? 1 : 0;
                dCount += value < prevValue ? 1 : 0;
            }

            double drp = dCount == length1 ? 1 : uCount == length1 ? -1 : 0;
            double drpPrice = drp != 0 ? currentValue : 0;
            drpPriceList.AddRounded(drpPrice);

            var signal = GetConditionSignal(drp > 0 || uCount > dCount, drp < 0 || dCount > uCount);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Drp", drpPriceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = drpPriceList;
        stockData.IndicatorName = IndicatorName.DemarkReversalPoints;

        return stockData;
    }

    /// <summary>
    /// Calculates the Demark Setup Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDemarkSetupIndicator(this StockData stockData, int length = 4)
    {
        List<double> drpPriceList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentValue = inputList[i];

            double uCount = 0, dCount = 0;
            for (int j = 0; j < length; j++)
            {
                double value = i >= j ? inputList[i - j] : 0;
                double prevValue = i >= j + length ? inputList[i - (j + length)] : 0;

                uCount += value > prevValue ? 1 : 0;
                dCount += value < prevValue ? 1 : 0;
            }

            double drp = dCount == length ? 1 : uCount == length ? -1 : 0;
            double drpPrice = drp != 0 ? currentValue : 0;
            drpPriceList.AddRounded(drpPrice);

            var signal = GetConditionSignal(drp > 0 || uCount > dCount, drp < 0 || dCount > uCount);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dsi", drpPriceList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = drpPriceList;
        stockData.IndicatorName = IndicatorName.DemarkSetupIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Demarker
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateDemarker(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 20)
    {
        List<double> demarkerList = new();
        List<double> dMaxList = new();
        List<double> dMinList = new();
        List<Signal> signalsList = new();
        var (_, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            double currentLow = lowList[i];
            double currentHigh = highList[i];
            double prevHigh = i >= 1 ? highList[i - 1] : 0;
            double prevLow = i >= 1 ? lowList[i - 1] : 0;

            double dMax = currentHigh > prevHigh ? currentHigh - prevHigh : 0;
            dMaxList.AddRounded(dMax);

            double dMin = currentLow < prevLow ? prevLow - currentLow : 0;
            dMinList.AddRounded(dMin);
        }

        var maxMaList = GetMovingAverageList(stockData, maType, length, dMaxList);
        var minMaList = GetMovingAverageList(stockData, maType, length, dMinList);
        for (int i = 0; i < stockData.Count; i++)
        {
            double maxMa = maxMaList[i];
            double minMa = minMaList[i];
            double prevDemarker1 = i >= 1 ? demarkerList[i - 1] : 0;
            double prevDemarker2 = i >= 2 ? demarkerList[i - 2] : 0;

            double demarker = maxMa + minMa != 0 ? MinOrMax(maxMa / (maxMa + minMa) * 100, 100, 0) : 0;
            demarkerList.AddRounded(demarker);

            var signal = GetRsiSignal(demarker - prevDemarker1, prevDemarker1 - prevDemarker2, demarker, prevDemarker1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Dm", demarkerList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = demarkerList;
        stockData.IndicatorName = IndicatorName.Demarker;

        return stockData;
    }
}