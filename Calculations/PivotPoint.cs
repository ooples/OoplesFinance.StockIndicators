namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the standard pivot points.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateStandardPivotPoints(this StockData stockData)
    {
        List<decimal> pivotList = new();
        List<decimal> resistanceLevel3List = new();
        List<decimal> resistanceLevel2List = new();
        List<decimal> resistanceLevel1List = new();
        List<decimal> supportLevel1List = new();
        List<decimal> supportLevel2List = new();
        List<decimal> supportLevel3List = new();
        List<decimal> midpoint1List = new();
        List<decimal> midpoint2List = new();
        List<decimal> midpoint3List = new();
        List<decimal> midpoint4List = new();
        List<decimal> midpoint5List = new();
        List<decimal> midpoint6List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevOpen = i >= 1 ? openList.ElementAtOrDefault(i - 1) : 0;

            decimal prevPivot = pivotList.LastOrDefault();
            decimal range = prevHigh - prevLow;
            decimal pivot = (prevHigh + prevLow + prevClose + prevOpen) / 4;
            pivotList.Add(pivot);

            decimal supportLevel1 = (pivot * 2) - prevHigh;
            supportLevel1List.Add(supportLevel1);

            decimal resistanceLevel1 = (pivot * 2) - prevLow;
            resistanceLevel1List.Add(resistanceLevel1);

            decimal range2 = resistanceLevel1 - supportLevel1;
            decimal supportLevel2 = pivot - range;
            supportLevel2List.Add(supportLevel2);

            decimal resistanceLevel2 = pivot + range;
            resistanceLevel2List.Add(resistanceLevel2);

            decimal supportLevel3 = pivot - range2;
            supportLevel3List.Add(supportLevel3);

            decimal resistanceLevel3 = pivot + range2;
            resistanceLevel3List.Add(resistanceLevel3);

            decimal midpoint1 = (supportLevel3 + supportLevel2) / 2;
            midpoint1List.Add(midpoint1);

            decimal midpoint2 = (supportLevel2 + supportLevel1) / 2;
            midpoint2List.Add(midpoint2);

            decimal midpoint3 = (supportLevel1 + pivot) / 2;
            midpoint3List.Add(midpoint3);

            decimal midpoint4 = (resistanceLevel1 + pivot) / 2;
            midpoint4List.Add(midpoint4);

            decimal midpoint5 = (resistanceLevel2 + resistanceLevel1) / 2;
            midpoint5List.Add(midpoint5);

            decimal midpoint6 = (resistanceLevel3 + resistanceLevel2) / 2;
            midpoint6List.Add(midpoint6);

            var signal = GetCompareSignal(currentClose - pivot, prevClose - prevPivot);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pivot", pivotList },
            { "S1", supportLevel1List },
            { "S2", supportLevel2List },
            { "S3", supportLevel3List },
            { "R1", resistanceLevel1List },
            { "R2", resistanceLevel2List },
            { "R3", resistanceLevel3List },
            { "M1", midpoint1List },
            { "M2", midpoint2List },
            { "M3", midpoint3List },
            { "M4", midpoint4List },
            { "M5", midpoint5List },
            { "M6", midpoint6List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pivotList;
        stockData.IndicatorName = IndicatorName.StandardPivotPoints;

        return stockData;
    }

    /// <summary>
    /// Calculates the Woodie Pivot Points
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateWoodiePivotPoints(this StockData stockData)
    {
        List<decimal> pivotList = new();
        List<decimal> resistanceLevel1List = new();
        List<decimal> resistanceLevel2List = new();
        List<decimal> resistanceLevel3List = new();
        List<decimal> resistanceLevel4List = new();
        List<decimal> supportLevel1List = new();
        List<decimal> supportLevel2List = new();
        List<decimal> supportLevel3List = new();
        List<decimal> supportLevel4List = new();
        List<decimal> midpoint1List = new();
        List<decimal> midpoint2List = new();
        List<decimal> midpoint3List = new();
        List<decimal> midpoint4List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevPivot = pivotList.LastOrDefault();
            decimal range = prevHigh - prevLow;
            decimal pivot = (prevHigh + prevLow + (prevClose * 2)) / 4;
            pivotList.Add(pivot);

            decimal supportLevel1 = (pivot * 2) - prevHigh;
            supportLevel1List.Add(supportLevel1);

            decimal resistanceLevel1 = (pivot * 2) - prevLow;
            resistanceLevel1List.Add(resistanceLevel1);

            decimal supportLevel2 = pivot - range;
            supportLevel2List.Add(supportLevel2);

            decimal resistanceLevel2 = pivot + range;
            resistanceLevel2List.Add(resistanceLevel2);

            decimal supportLevel3 = prevLow - (2 * (prevHigh - pivot));
            supportLevel3List.Add(supportLevel3);

            decimal resistanceLevel3 = prevHigh + (2 * (pivot - prevLow));
            resistanceLevel3List.Add(resistanceLevel3);

            decimal supportLevel4 = supportLevel3 - range;
            supportLevel4List.Add(supportLevel4);

            decimal resistanceLevel4 = resistanceLevel3 + range;
            resistanceLevel4List.Add(resistanceLevel4);

            decimal midpoint1 = (supportLevel1 + supportLevel2) / 2;
            midpoint1List.Add(midpoint1);

            decimal midpoint2 = (pivot + supportLevel1) / 2;
            midpoint2List.Add(midpoint2);

            decimal midpoint3 = (resistanceLevel1 + pivot) / 2;
            midpoint3List.Add(midpoint3);

            decimal midpoint4 = (resistanceLevel1 + resistanceLevel2) / 2;
            midpoint4List.Add(midpoint4);

            var signal = GetCompareSignal(currentClose - pivot, prevClose - prevPivot);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pivot", pivotList },
            { "S1", supportLevel1List },
            { "S2", supportLevel2List },
            { "S3", supportLevel3List },
            { "S4", supportLevel4List },
            { "R1", resistanceLevel1List },
            { "R2", resistanceLevel2List },
            { "R3", resistanceLevel3List },
            { "R4", resistanceLevel4List },
            { "M1", midpoint1List },
            { "M2", midpoint2List },
            { "M3", midpoint3List },
            { "M4", midpoint4List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pivotList;
        stockData.IndicatorName = IndicatorName.WoodiePivotPoints;

        return stockData;
    }

    /// <summary>
    /// Calculates the Floor Pivot Points
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateFloorPivotPoints(this StockData stockData)
    {
        List<decimal> pivotList = new();
        List<decimal> resistanceLevel3List = new();
        List<decimal> resistanceLevel2List = new();
        List<decimal> resistanceLevel1List = new();
        List<decimal> supportLevel1List = new();
        List<decimal> supportLevel2List = new();
        List<decimal> supportLevel3List = new();
        List<decimal> midpoint1List = new();
        List<decimal> midpoint2List = new();
        List<decimal> midpoint3List = new();
        List<decimal> midpoint4List = new();
        List<decimal> midpoint5List = new();
        List<decimal> midpoint6List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i) : 0;
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i) : 0;

            decimal range = prevHigh - prevLow;
            decimal pivot = (prevHigh + prevLow + prevClose) / 3;
            pivotList.Add(pivot);

            decimal prevSupportLevel1 = supportLevel1List.LastOrDefault();
            decimal supportLevel1 = (pivot * 2) - prevHigh;
            supportLevel1List.Add(supportLevel1);

            decimal prevResistanceLevel1 = resistanceLevel1List.LastOrDefault();
            decimal resistanceLevel1 = (pivot * 2) - prevLow;
            resistanceLevel1List.Add(resistanceLevel1);

            decimal supportLevel2 = pivot - range;
            supportLevel2List.Add(supportLevel2);

            decimal resistanceLevel2 = pivot + range;
            resistanceLevel2List.Add(resistanceLevel2);

            decimal supportLevel3 = supportLevel1 - range;
            supportLevel3List.Add(supportLevel3);

            decimal resistanceLevel3 = resistanceLevel1 + range;
            resistanceLevel3List.Add(resistanceLevel3);

            decimal midpoint1 = (supportLevel3 + supportLevel2) / 2;
            midpoint1List.Add(midpoint1);

            decimal midpoint2 = (supportLevel2 + supportLevel1) / 2;
            midpoint2List.Add(midpoint2);

            decimal midpoint3 = (supportLevel1 + pivot) / 2;
            midpoint3List.Add(midpoint3);

            decimal midpoint4 = (resistanceLevel1 + pivot) / 2;
            midpoint4List.Add(midpoint4);

            decimal midpoint5 = (resistanceLevel2 + resistanceLevel1) / 2;
            midpoint5List.Add(midpoint5);

            decimal midpoint6 = (resistanceLevel3 + resistanceLevel2) / 2;
            midpoint6List.Add(midpoint6);

            var signal = GetBullishBearishSignal(currentClose - resistanceLevel1, prevClose - prevResistanceLevel1,
                currentClose - supportLevel1, prevClose - prevSupportLevel1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pivot", pivotList },
            { "S1", supportLevel1List },
            { "S2", supportLevel2List },
            { "S3", supportLevel3List },
            { "R1", resistanceLevel1List },
            { "R2", resistanceLevel2List },
            { "R3", resistanceLevel3List },
            { "M1", midpoint1List },
            { "M2", midpoint2List },
            { "M3", midpoint3List },
            { "M4", midpoint4List },
            { "M5", midpoint5List },
            { "M6", midpoint6List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pivotList;
        stockData.IndicatorName = IndicatorName.FloorPivotPoints;

        return stockData;
    }

    /// <summary>
    /// Calculates the Fibonacci Pivot Points
    /// </summary>
    /// <param name="stockData"></param>
    /// <returns></returns>
    public static StockData CalculateFibonacciPivotPoints(this StockData stockData)
    {
        List<decimal> pivotList = new();
        List<decimal> resistanceLevel3List = new();
        List<decimal> resistanceLevel2List = new();
        List<decimal> resistanceLevel1List = new();
        List<decimal> supportLevel1List = new();
        List<decimal> supportLevel2List = new();
        List<decimal> supportLevel3List = new();
        List<decimal> midpoint1List = new();
        List<decimal> midpoint2List = new();
        List<decimal> midpoint3List = new();
        List<decimal> midpoint4List = new();
        List<decimal> midpoint5List = new();
        List<decimal> midpoint6List = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal prevClose = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevLow = i >= 1 ? lowList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHigh = i >= 1 ? highList.ElementAtOrDefault(i - 1) : 0;

            decimal prevPivot = pivotList.LastOrDefault();
            decimal range = prevHigh - prevLow;
            decimal pivot = (prevHigh + prevLow + prevClose) / 3;
            pivotList.Add(pivot);

            decimal supportLevel1 = pivot - (range * 0.382m);
            supportLevel1List.Add(supportLevel1);

            decimal supportLevel2 = pivot - (range * 0.618m);
            supportLevel2List.Add(supportLevel2);

            decimal supportLevel3 = pivot - (range * 1);
            supportLevel3List.Add(supportLevel3);

            decimal resistanceLevel1 = pivot + (range * 0.382m);
            resistanceLevel1List.Add(resistanceLevel1);

            decimal resistanceLevel2 = pivot + (range * 0.618m);
            resistanceLevel2List.Add(resistanceLevel2);

            decimal resistanceLevel3 = pivot + (range * 1);
            resistanceLevel3List.Add(resistanceLevel3);

            decimal midpoint1 = (supportLevel3 + supportLevel2) / 2;
            midpoint1List.Add(midpoint1);

            decimal midpoint2 = (supportLevel2 + supportLevel1) / 2;
            midpoint2List.Add(midpoint2);

            decimal midpoint3 = (supportLevel1 + pivot) / 2;
            midpoint3List.Add(midpoint3);

            decimal midpoint4 = (resistanceLevel1 + pivot) / 2;
            midpoint4List.Add(midpoint4);

            decimal midpoint5 = (resistanceLevel2 + resistanceLevel1) / 2;
            midpoint5List.Add(midpoint5);

            decimal midpoint6 = (resistanceLevel3 + resistanceLevel2) / 2;
            midpoint6List.Add(midpoint6);

            var signal = GetCompareSignal(currentClose - pivot, prevClose - prevPivot);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Pivot", pivotList },
            { "S1", supportLevel1List },
            { "S2", supportLevel2List },
            { "S3", supportLevel3List },
            { "R1", resistanceLevel1List },
            { "R2", resistanceLevel2List },
            { "R3", resistanceLevel3List },
            { "M1", midpoint1List },
            { "M2", midpoint2List },
            { "M3", midpoint3List },
            { "M4", midpoint4List },
            { "M5", midpoint5List },
            { "M6", midpoint6List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = pivotList;
        stockData.IndicatorName = IndicatorName.FibonacciPivotPoints;

        return stockData;
    }
}