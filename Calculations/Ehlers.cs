namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the Ehlers Roofing Filter
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

    /// <summary>
    /// Calculates the Ehlers Phase Calculation
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersPhaseCalculation(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int length = 15)
    {
        List<decimal> phaseList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal realPart = 0, imagPart = 0;
            for (int j = 0; j < length; j++)
            {
                decimal weight = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                realPart += Cos(2 * Pi * (decimal)j / length) * weight;
                imagPart += Sin(2 * Pi * (decimal)j / length) * weight;
            }

            decimal phase = Math.Abs(realPart) > 0.001m ? Atan(imagPart / realPart).ToDegrees() : 90 * Math.Sign(imagPart);
            phase = realPart < 0 ? phase + 180 : phase;
            phase += 90;
            phase = phase < 0 ? phase + 360 : phase;
            phase = phase > 360 ? phase - 360 : phase;
            phaseList.AddRounded(phase);
        }

        var phaseEmaList = GetMovingAverageList(stockData, maType, length, phaseList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal phase = phaseList.ElementAtOrDefault(i);
            decimal phaseEma = phaseEmaList.ElementAtOrDefault(i);
            decimal prevPhase = i >= 1 ? phaseList.ElementAtOrDefault(i - 1) : 0;
            decimal prevPhaseEma = i >= 1 ? phaseEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(phase - phaseEma, prevPhase - prevPhaseEma, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Phase", phaseList },
            { "Signal", phaseEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = phaseList;
        stockData.IndicatorName = IndicatorName.EhlersPhaseCalculation;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Adaptive Cyber Cycle
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersAdaptiveCyberCycle(this StockData stockData, int length = 5, decimal alpha = 0.07m)
    {
        List<decimal> ipList = new();
        List<decimal> q1List = new();
        List<decimal> i1List = new();
        List<decimal> dpList = new();
        List<decimal> pList = new();
        List<decimal> acList = new();
        List<decimal> cycleList = new();
        List<decimal> smoothList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCycle = i >= 1 ? cycleList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSmooth = i >= 1 ? smoothList.ElementAtOrDefault(i - 1) : 0;
            decimal prevIp = i >= 1 ? ipList.ElementAtOrDefault(i - 1) : 0;
            decimal prevAc1 = i >= 1 ? acList.ElementAtOrDefault(i - 1) : 0;
            decimal prevI1 = i >= 1 ? i1List.ElementAtOrDefault(i - 1) : 0;
            decimal prevQ1 = i >= 1 ? q1List.ElementAtOrDefault(i - 1) : 0;
            decimal prevP = i >= 1 ? pList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevSmooth2 = i >= 2 ? smoothList.ElementAtOrDefault(i - 2) : 0;
            decimal prevCycle2 = i >= 2 ? cycleList.ElementAtOrDefault(i - 2) : 0;
            decimal prevAc2 = i >= 2 ? acList.ElementAtOrDefault(i - 2) : 0;
            decimal prevValue3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
            decimal prevCycle3 = i >= 3 ? cycleList.ElementAtOrDefault(i - 3) : 0;
            decimal prevCycle4 = i >= 4 ? cycleList.ElementAtOrDefault(i - 4) : 0;
            decimal prevCycle6 = i >= 6 ? cycleList.ElementAtOrDefault(i - 6) : 0;

            decimal smooth = (currentValue + (2 * prevValue) + (2 * prevValue2) + prevValue3) / 6;
            smoothList.Add(smooth);

            decimal cycle = i < 7 ? (currentValue - (2 * prevValue) + prevValue2) / 4 : (Pow(1 - (0.5m * alpha), 2) * (smooth - (2 * prevSmooth) + prevSmooth2)) +
            (2 * (1 - alpha) * prevCycle) - (Pow(1 - alpha, 2) * prevCycle2);
            cycleList.Add(cycle);

            decimal q1 = ((0.0962m * cycle) + (0.5769m * prevCycle2) - (0.5769m * prevCycle4) - (0.0962m * prevCycle6)) * (0.5m + (0.08m * prevIp));
            q1List.Add(q1);

            decimal i1 = prevCycle3;
            i1List.Add(i1);

            decimal dp = MinOrMax(q1 != 0 && prevQ1 != 0 ? ((i1 / q1) - (prevI1 / prevQ1)) / (1 + (i1 * prevI1 / (q1 * prevQ1))) : 0, 1.1m, 0.1m);
            dpList.Add(dp);

            decimal medianDelta = dpList.TakeLastExt(length).Median();
            decimal dc = medianDelta != 0 ? (6.28318m / medianDelta) + 0.5m : 15;

            decimal ip = (0.33m * dc) + (0.67m * prevIp);
            ipList.Add(ip);

            decimal p = (0.15m * ip) + (0.85m * prevP);
            pList.Add(p);

            decimal a1 = 2 / (p + 1);
            decimal ac = i < 7 ? (currentValue - (2 * prevValue) + prevValue2) / 4 :
                (Pow(1 - (0.5m * a1), 2) * (smooth - (2 * prevSmooth) + prevSmooth2)) + (2 * (1 - a1) * prevAc1) - (Pow(1 - a1, 2) * prevAc2);
            acList.Add(ac);

            var signal = GetCompareSignal(ac - prevAc1, prevAc1 - prevAc2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eacc", acList },
            { "Period", pList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = acList;
        stockData.IndicatorName = IndicatorName.EhlersAdaptiveCyberCycle;

        return stockData;
    }
}