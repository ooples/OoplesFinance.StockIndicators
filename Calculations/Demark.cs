namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    public static StockData CalculateDemarkRangeExpansionIndex(this StockData stockData, int length = 5)
    {
        List<decimal> s2List = new();
        List<decimal> s1List = new();
        List<decimal> reiList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal high = highList.ElementAtOrDefault(i);
            decimal prevHigh2 = i >= 2 ? highList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHigh5 = i >= 5 ? highList.ElementAtOrDefault(i - 5) : 0;
            decimal prevHigh6 = i >= 6 ? highList.ElementAtOrDefault(i - 6) : 0;
            decimal low = lowList.ElementAtOrDefault(i);
            decimal prevLow2 = i >= 2 ? lowList.ElementAtOrDefault(i - 2) : 0;
            decimal prevLow5 = i >= 5 ? lowList.ElementAtOrDefault(i - 5) : 0;
            decimal prevLow6 = i >= 6 ? lowList.ElementAtOrDefault(i - 6) : 0;
            decimal prevClose7 = i >= 7 ? inputList.ElementAtOrDefault(i - 7) : 0;
            decimal prevClose8 = i >= 8 ? inputList.ElementAtOrDefault(i - 8) : 0;
            decimal prevRei1 = i >= 1 ? reiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRei2 = i >= 2 ? reiList.ElementAtOrDefault(i - 2) : 0;
            decimal n = (high >= prevLow5 || high >= prevLow6) && (low <= prevHigh5 || low <= prevHigh6) ? 0 : 1;
            decimal m = prevHigh2 >= prevClose8 && (prevLow2 <= prevClose7 || prevLow2 <= prevClose8) ? 0 : 1;
            decimal s = high - prevHigh2 + (low - prevLow2);

            decimal s1 = n * m * s;
            s1List.AddRounded(s1);

            decimal s2 = Math.Abs(s);
            s2List.AddRounded(s2);

            decimal s1Sum = s1List.TakeLastExt(length).Sum();
            decimal s2Sum = s2List.TakeLastExt(length).Sum();

            decimal rei = s2Sum != 0 ? s1Sum / s2Sum * 100 : 0;
            reiList.Add(rei);

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
}