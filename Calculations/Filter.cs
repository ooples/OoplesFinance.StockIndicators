namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the ehlers roofing filter.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <returns></returns>
    public static StockData CalculateEhlersRoofingFilter(this StockData stockData)
    {
        int lowerLength = 10, upperLength = 48;

        return CalculateEhlersRoofingFilter(stockData, lowerLength, upperLength);
    }

    /// <summary>
    /// Calculates the ehlers roofing filter.
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="lowerLength">Length of the lower.</param>
    /// <param name="upperLength">Length of the upper.</param>
    /// <returns></returns>
    public static StockData CalculateEhlersRoofingFilter(this StockData stockData, int lowerLength, int upperLength)
    {
        List<decimal> highPassList = new();
        List<decimal> roofingFilterList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alphaArg = Math.Min(0.707m * 2 * Pi / upperLength, 0.99m);
        decimal alphaCos = Cos(alphaArg);
        decimal alpha1 = alphaCos != 0 ? (alphaCos + Sin(alphaArg) - 1) / alphaCos : 0;
        decimal a1 = Exp(-1.414m * Pi / lowerLength);
        decimal b1 = 2 * a1 * Cos(Math.Min(1.414m * Pi / lowerLength, 0.99m));
        decimal c2 = b1;
        decimal c3 = -1 * a1 * a1;
        decimal c1 = 1 - c2 - c3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevFilter1 = i >= 1 ? roofingFilterList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFilter2 = i >= 2 ? roofingFilterList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHp1 = i >= 1 ? highPassList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHp2 = i >= 2 ? highPassList.ElementAtOrDefault(i - 2) : 0;
            decimal test1 = Pow((1 - alpha1) / 2, 2);
            decimal test2 = currentValue - (2 * prevValue1) + prevValue2;
            decimal v1 = test1 * test2;
            decimal v2 = 2 * (1 - alpha1) * prevHp1;
            decimal v3 = Pow(1 - alpha1, 2) * prevHp2;

            decimal highPass = v1 + v2 - v3;
            highPassList.Add(highPass);

            decimal prevRoofingFilter1 = i >= 1 ? roofingFilterList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRoofingFilter2 = i >= 2 ? roofingFilterList.ElementAtOrDefault(i - 2) : 0;
            decimal roofingFilter = (c1 * ((highPass + prevHp1) / 2)) + (c2 * prevFilter1) + (c3 * prevFilter2);
            roofingFilterList.Add(roofingFilter);

            var signal = GetCompareSignal(roofingFilter - prevRoofingFilter1, prevRoofingFilter1 - prevRoofingFilter2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Erf", roofingFilterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = roofingFilterList;
        stockData.IndicatorName = IndicatorName.EhlersRoofingFilter;

        return stockData;
    }
}