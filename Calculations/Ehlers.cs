namespace OoplesFinance.StockIndicators;

public static partial class Calculations
{
    /// <summary>
    /// Calculates the Ehlers Roofing Filter V2
    /// </summary>
    /// <param name="stockData">The stock data.</param>
    /// <param name="lowerLength">Length of the lower.</param>
    /// <param name="upperLength">Length of the upper.</param>
    /// <returns></returns>
    public static StockData CalculateEhlersRoofingFilterV2(this StockData stockData, int upperLength = 80, int lowerLength = 40)
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
        stockData.IndicatorName = IndicatorName.EhlersRoofingFilterV2;

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

    /// <summary>
    /// Calculates the Ehlers Simple Decycler
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="upperPct"></param>
    /// <param name="lowerPct"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersSimpleDecycler(this StockData stockData, int length = 125, decimal upperPct = 0.5m, decimal lowerPct = 0.5m)
    {
        List<decimal> decyclerList = new();
        List<decimal> upperBandList = new();
        List<decimal> lowerBandList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var hpList = CalculateEhlersHighPassFilterV1(stockData, length, 1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal hp = hpList.ElementAtOrDefault(i);

            decimal prevDecycler = decyclerList.LastOrDefault();
            decimal decycler = currentValue - hp;
            decyclerList.Add(decycler);

            decimal upperBand = (1 + (upperPct / 100)) * decycler;
            upperBandList.Add(upperBand);

            decimal lowerBand = (1 - (lowerPct / 100)) * decycler;
            lowerBandList.Add(lowerBand);

            var signal = GetCompareSignal(currentValue - decycler, prevValue - prevDecycler);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "UpperBand", upperBandList },
            { "MiddleBand", decyclerList },
            { "LowerBand", lowerBandList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = decyclerList;
        stockData.IndicatorName = IndicatorName.EhlersSimpleDecycler;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers High Pass Filter V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersHighPassFilterV1(this StockData stockData, int length = 125, decimal mult = 1)
    {
        List<decimal> highPassList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alphaArg = MinOrMax(2 * Pi / (mult * length * Sqrt(2)), 0.99m, 0.01m);
        decimal alphaCos = Cos(alphaArg);
        decimal alpha = alphaCos != 0 ? (alphaCos + Sin(alphaArg) - 1) / alphaCos : 0;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHp1 = i >= 1 ? highPassList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHp2 = i >= 2 ? highPassList.ElementAtOrDefault(i - 2) : 0;
            decimal pow1 = Pow(1 - (alpha / 2), 2);
            decimal pow2 = Pow(1 - alpha, 2);

            decimal highPass = (pow1 * (currentValue - (2 * prevValue1) + prevValue2)) + (2 * (1 - alpha) * prevHp1) - (pow2 * prevHp2);
            highPassList.Add(highPass);

            var signal = GetCompareSignal(highPass, prevHp1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Hp", highPassList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = highPassList;
        stockData.IndicatorName = IndicatorName.EhlersHighPassFilterV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Rocket Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="obosLevel"></param>
    /// <param name="mult"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersRocketRelativeStrengthIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.Ehlers2PoleSuperSmootherFilterV2, 
        int length1 = 10, int length2 = 8, decimal obosLevel = 2, decimal mult = 1)
    {
        List<decimal> momList = new();
        List<decimal> argList = new();
        List<decimal> ssf2PoleRocketRsiList = new();
        List<decimal> ssf2PoleUpChgList = new();
        List<decimal> ssf2PoleDownChgList = new();
        List<decimal> ssf2PoleTmpList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal obLevel = obosLevel * mult;
        decimal osLevel = -obosLevel * mult;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= length1 - 1 ? inputList.ElementAtOrDefault(i - (length1 - 1)) : 0;

            decimal prevMom = momList.LastOrDefault();
            decimal mom = currentValue - prevValue;
            momList.Add(mom);

            decimal arg = (mom + prevMom) / 2;
            argList.Add(arg);
        }

        var argSsf2PoleList = GetMovingAverageList(stockData, maType, length2, argList);
        for (int j = 0; j < stockData.Count; j++)
        {
            decimal ssf2Pole = argSsf2PoleList.ElementAtOrDefault(j);
            decimal prevSsf2Pole = j >= 1 ? argSsf2PoleList.ElementAtOrDefault(j - 1) : 0;
            decimal prevRocketRsi1 = j >= 1 ? ssf2PoleRocketRsiList.ElementAtOrDefault(j - 1) : 0;
            decimal prevRocketRsi2 = j >= 2 ? ssf2PoleRocketRsiList.ElementAtOrDefault(j - 2) : 0;
            decimal ssf2PoleMom = ssf2Pole - prevSsf2Pole;

            decimal up2PoleChg = ssf2PoleMom > 0 ? ssf2PoleMom : 0;
            ssf2PoleUpChgList.Add(up2PoleChg);

            decimal down2PoleChg = ssf2PoleMom < 0 ? Math.Abs(ssf2PoleMom) : 0;
            ssf2PoleDownChgList.Add(down2PoleChg);

            decimal up2PoleChgSum = ssf2PoleUpChgList.TakeLastExt(length1).Sum();
            decimal down2PoleChgSum = ssf2PoleDownChgList.TakeLastExt(length1).Sum();

            decimal prevTmp2Pole = ssf2PoleTmpList.LastOrDefault();
            decimal tmp2Pole = up2PoleChgSum + down2PoleChgSum != 0 ?
                MinOrMax((up2PoleChgSum - down2PoleChgSum) / (up2PoleChgSum + down2PoleChgSum), 0.999m, -0.999m) : prevTmp2Pole;
            ssf2PoleTmpList.Add(tmp2Pole);

            decimal ssf2PoleTempLog = 1 - tmp2Pole != 0 ? (1 + tmp2Pole) / (1 - tmp2Pole) : 0;
            decimal ssf2PoleLog = Log(ssf2PoleTempLog);
            decimal ssf2PoleRocketRsi = 0.5m * ssf2PoleLog * mult;
            ssf2PoleRocketRsiList.Add(ssf2PoleRocketRsi);

            var signal = GetRsiSignal(ssf2PoleRocketRsi - prevRocketRsi1, prevRocketRsi1 - prevRocketRsi2, ssf2PoleRocketRsi, prevRocketRsi1, obLevel, osLevel);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Errsi", ssf2PoleRocketRsiList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = ssf2PoleRocketRsiList;
        stockData.IndicatorName = IndicatorName.EhlersRocketRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Correlation Trend Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersCorrelationTrendIndicator(this StockData stockData, int length = 20)
    {
        List<decimal> corrList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevCorr1 = i >= 1 ? corrList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCorr2 = i >= 2 ? corrList.ElementAtOrDefault(i - 2) : 0;

            decimal sx = 0, sy = 0, sxx = 0, sxy = 0, syy = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal x = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                decimal y = -j;

                sx += x;
                sy += y;
                sxx += Pow(x, 2);
                sxy += x * y;
                syy += Pow(y, 2);
            }

            decimal corr = (length * sxx) - (sx * sx) > 0 && (length * syy) - (sy * sy) > 0 ? ((length * sxy) - (sx * sy)) /
                Sqrt(((length * sxx) - (sx * sx)) * ((length * syy) - (sy * sy))) : 0;
            corrList.Add(corr);

            var signal = GetRsiSignal(corr - prevCorr1, prevCorr1 - prevCorr2, corr, prevCorr1, 0.5m, -0.5m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ecti", corrList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = corrList;
        stockData.IndicatorName = IndicatorName.EhlersCorrelationTrendIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Relative Vigor Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="signalLength"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersRelativeVigorIndex(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage, int length = 10,
        int signalLength = 4)
    {
        List<decimal> rviList = new();
        List<Signal> signalsList = new();
        var (inputList, highList, lowList, openList, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentClose = inputList.ElementAtOrDefault(i);
            decimal currentOpen = openList.ElementAtOrDefault(i);
            decimal currentHigh = highList.ElementAtOrDefault(i);
            decimal currentLow = lowList.ElementAtOrDefault(i);

            decimal rvi = currentHigh - currentLow != 0 ? (currentClose - currentOpen) / (currentHigh - currentLow) : 0;
            rviList.Add(rvi);
        }

        var rviSmaList = GetMovingAverageList(stockData, maType, length, rviList);
        var rviSignalList = GetMovingAverageList(stockData, maType, signalLength, rviSmaList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rviSma = rviSmaList.ElementAtOrDefault(i);
            decimal prevRviSma = i >= 1 ? rviSmaList.ElementAtOrDefault(i - 1) : 0;
            decimal rviSignal = rviSignalList.ElementAtOrDefault(i);
            decimal prevRviSignal = i >= 1 ? rviSignalList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(rviSma - rviSignal, prevRviSma - prevRviSignal);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ervi", rviSmaList },
            { "Signal", rviSignalList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rviSmaList;
        stockData.IndicatorName = IndicatorName.EhlersRelativeVigorIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Center Of Gravity
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersCenterofGravityOscillator(this StockData stockData, int length = 10)
    {
        List<decimal> cgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal num = 0, denom = 0;
            for (int j = 0; j <= length - 1; j++)
            {
                decimal prevValue = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                num += (1 + j) * prevValue;
                denom += prevValue;
            }

            decimal prevCg = cgList.LastOrDefault();
            decimal cg = denom != 0 ? (-num / denom) + ((decimal)(length + 1) / 2) : 0;
            cgList.Add(cg);

            var signal = GetCompareSignal(cg, prevCg);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ecog", cgList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cgList;
        stockData.IndicatorName = IndicatorName.EhlersCenterofGravityOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Adaptive Center Of Gravity Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersAdaptiveCenterOfGravityOscillator(this StockData stockData, int length = 5)
    {
        List<decimal> cgList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var pList = CalculateEhlersAdaptiveCyberCycle(stockData, length: length).OutputValues["Period"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal p = pList.ElementAtOrDefault(i);
            int intPeriod = (int)Math.Ceiling(p / 2);
            decimal prevCg1 = i >= 1 ? cgList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCg2 = i >= 2 ? cgList.ElementAtOrDefault(i - 2) : 0;

            decimal num = 0, denom = 0;
            for (int j = 0; j <= intPeriod - 1; j++)
            {
                decimal prevPrice = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                num += (1 + j) * prevPrice;
                denom += prevPrice;
            }

            decimal cg = denom != 0 ? (-num / denom) + ((intPeriod + 1) / 2) : 0;
            cgList.Add(cg);

            var signal = GetCompareSignal(cg - prevCg1, prevCg1 - prevCg2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eacog", cgList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cgList;
        stockData.IndicatorName = IndicatorName.EhlersAdaptiveCenterOfGravityOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Smoothed Adaptive Momentum
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersSmoothedAdaptiveMomentum(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage, 
        int length1 = 5, int length2 = 8)
    {
        List<decimal> f3List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(-Pi / length2);
        decimal b1 = 2 * a1 * Cos(1.738m * Pi / length2);
        decimal c1 = Pow(a1, 2);
        decimal coef2 = b1 + c1;
        decimal coef3 = -1 * (c1 + (b1 * c1));
        decimal coef4 = c1 * c1;
        decimal coef1 = 1 - coef2 - coef3 - coef4;

        var pList = CalculateEhlersAdaptiveCyberCycle(stockData, length1).OutputValues["Period"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevP = i >= 1 ? pList.ElementAtOrDefault(i - 1) : 0;
            decimal p = pList.ElementAtOrDefault(i);
            decimal prevF3_1 = i >= 1 ? f3List.ElementAtOrDefault(i - 1) : 0;
            decimal prevF3_2 = i >= 2 ? f3List.ElementAtOrDefault(i - 2) : 0;
            decimal prevF3_3 = i >= 3 ? f3List.ElementAtOrDefault(i - 3) : 0;
            int pr = (int)Math.Ceiling(Math.Abs(p - 1));
            decimal prevValue = i >= pr ? inputList.ElementAtOrDefault(i - pr) : 0;
            decimal v1 = currentValue - prevValue;

            decimal f3 = (coef1 * v1) + (coef2 * prevF3_1) + (coef3 * prevF3_2) + (coef4 * prevF3_3);
            f3List.Add(f3);
        }

        var f3EmaList = GetMovingAverageList(stockData, maType, length2, f3List);
        for (int j = 0; j < stockData.Count; j++)
        {
            decimal f3 = f3List.ElementAtOrDefault(j);
            decimal f3Ema = f3EmaList.ElementAtOrDefault(j);
            decimal prevF3 = j >= 1 ? f3List.ElementAtOrDefault(j - 1) : 0;
            decimal prevF3Ema = j >= 1 ? f3EmaList.ElementAtOrDefault(j - 1) : 0;

            var signal = GetCompareSignal(f3 - f3Ema, prevF3 - prevF3Ema);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Esam", f3List },
            { "Signal", f3EmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = f3List;
        stockData.IndicatorName = IndicatorName.EhlersSmoothedAdaptiveMomentumIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Stochastic Center Of Gravity Oscillator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersStochasticCenterOfGravityOscillator(this StockData stockData, int length = 8)
    {
        List<decimal> v1List = new();
        List<decimal> v2List = new();
        List<decimal> tList = new();
        List<Signal> signalsList = new();

        var ehlersCGOscillatorList = CalculateEhlersCenterofGravityOscillator(stockData, length).CustomValuesList;
        var (highestList, lowestList) = GetMaxAndMinValuesList(ehlersCGOscillatorList, length);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal cg = ehlersCGOscillatorList.ElementAtOrDefault(i);
            decimal maxc = highestList.ElementAtOrDefault(i);
            decimal minc = lowestList.ElementAtOrDefault(i);
            decimal prevV1_1 = i >= 1 ? v1List.ElementAtOrDefault(i - 1) : 0;
            decimal prevV1_2 = i >= 2 ? v1List.ElementAtOrDefault(i - 2) : 0;
            decimal prevV1_3 = i >= 3 ? v1List.ElementAtOrDefault(i - 3) : 0;
            decimal prevV2_1 = i >= 1 ? v2List.ElementAtOrDefault(i - 1) : 0;
            decimal prevT1 = i >= 1 ? tList.ElementAtOrDefault(i - 1) : 0;
            decimal prevT2 = i >= 2 ? tList.ElementAtOrDefault(i - 2) : 0;

            decimal v1 = maxc - minc != 0 ? (cg - minc) / (maxc - minc) : 0;
            v1List.Add(v1);

            decimal v2_ = ((4 * v1) + (3 * prevV1_1) + (2 * prevV1_2) + prevV1_3) / 10;
            decimal v2 = 2 * (v2_ - 0.5m);
            v2List.Add(v2);

            decimal t = MinOrMax(0.96m * (prevV2_1 + 0.02m), 1, 0);
            tList.Add(t);

            var signal = GetRsiSignal(t - prevT1, prevT1 - prevT2, t, prevT1, 0.8m, 0.2m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Escog", tList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = tList;
        stockData.IndicatorName = IndicatorName.EhlersStochasticCenterOfGravityOscillator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Simple Cycle Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersSimpleCycleIndicator(this StockData stockData, decimal alpha = 0.07m)
    {
        List<decimal> smoothList = new();
        List<decimal> cycleList = new();
        List<decimal> cycle_List = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentMedianPrice = inputList.ElementAtOrDefault(i);
            decimal prevMedianPrice1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMedianPrice2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevMedianPrice3 = i >= 3 ? inputList.ElementAtOrDefault(i - 3) : 0;
            decimal prevSmooth1 = smoothList.LastOrDefault();
            decimal prevCycle1 = cycle_List.LastOrDefault();
            decimal prevSmooth2 = i >= 2 ? smoothList.ElementAtOrDefault(i - 2) : 0;
            decimal prevCycle2 = i >= 2 ? cycle_List.ElementAtOrDefault(i - 2) : 0;
            decimal prevCyc1 = i >= 1 ? cycleList.ElementAtOrDefault(i - 1) : 0;
            decimal prevCyc2 = i >= 2 ? cycleList.ElementAtOrDefault(i - 2) : 0;

            decimal smooth = (currentMedianPrice + (2 * prevMedianPrice1) + (2 * prevMedianPrice2) + prevMedianPrice3) / 6;
            smoothList.Add(smooth);

            decimal cycle_ = ((1 - (0.5m * alpha)) * (1 - (0.5m * alpha)) * (smooth - (2 * prevSmooth1) + prevSmooth2)) + (2 * (1 - alpha) * prevCycle1) -
                ((1 - alpha) * (1 - alpha) * prevCycle2);
            cycle_List.Add(cycle_);

            decimal cycle = i < 7 ? (currentMedianPrice - (2 * prevMedianPrice1) + prevMedianPrice2) / 4 : cycle_;
            cycleList.Add(cycle);

            var signal = GetCompareSignal(cycle - prevCyc1, prevCyc1 - prevCyc2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Esci", cycleList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = cycleList;
        stockData.IndicatorName = IndicatorName.EhlersSimpleCycleIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Decycler Oscillator V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <param name="fastMult"></param>
    /// <param name="slowMult"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersDecyclerOscillatorV1(this StockData stockData, int fastLength = 100, int slowLength = 125, 
        decimal fastMult = 1.2m, decimal slowMult = 1)
    {
        List<decimal> decycler1OscillatorList = new();
        List<decimal> decycler2OscillatorList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        var decycler1List = CalculateEhlersSimpleDecycler(stockData, fastLength).CustomValuesList;
        var decycler2List = CalculateEhlersSimpleDecycler(stockData, slowLength).CustomValuesList;
        stockData.CustomValuesList = decycler1List;
        var decycler1FilteredList = CalculateEhlersHighPassFilterV1(stockData, fastLength, 0.5m).CustomValuesList;
        stockData.CustomValuesList = decycler2List;
        var decycler2FilteredList = CalculateEhlersHighPassFilterV1(stockData, slowLength, 0.5m).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal decycler1Filtered = decycler1FilteredList.ElementAtOrDefault(i);
            decimal decycler2Filtered = decycler2FilteredList.ElementAtOrDefault(i);

            decimal prevDecyclerOsc1 = decycler1OscillatorList.LastOrDefault();
            decimal decyclerOscillator1 = currentValue != 0 ? 100 * fastMult * decycler1Filtered / currentValue : 0;
            decycler1OscillatorList.Add(decyclerOscillator1);

            decimal prevDecyclerOsc2 = decycler2OscillatorList.LastOrDefault();
            decimal decyclerOscillator2 = currentValue != 0 ? 100 * slowMult * decycler2Filtered / currentValue : 0;
            decycler2OscillatorList.Add(decyclerOscillator2);

            var signal = GetCompareSignal(decyclerOscillator2 - decyclerOscillator1, prevDecyclerOsc2 - prevDecyclerOsc1);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "FastEdo", decycler1OscillatorList },
            { "SlowEdo", decycler2OscillatorList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.EhlersDecyclerOscillatorV1;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers High Pass Filter V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersHighPassFilterV2(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage, int length = 20)
    {
        List<decimal> hpList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(-1.414m * Pi / length);
        decimal b1 = 2 * a1 * Cos(1.414m * Pi / length);
        decimal c2 = b1;
        decimal c3 = -a1 * a1;
        decimal c1 = (1 + c2 - c3) / 4;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHp1 = i >= 1 ? hpList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHp2 = i >= 2 ? hpList.ElementAtOrDefault(i - 2) : 0;

            decimal hp = i < 4 ? 0 : (c1 * (currentValue - (2 * prevValue1) + prevValue2)) + (c2 * prevHp1) + (c3 * prevHp2);
            hpList.Add(hp);
        }

        var hpMa1List = GetMovingAverageList(stockData, maType, length, hpList);
        var hpMa2List = GetMovingAverageList(stockData, maType, length, hpMa1List);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal hp = hpMa2List.ElementAtOrDefault(i);
            decimal prevHp1 = i >= 1 ? hpMa2List.ElementAtOrDefault(i - 1) : 0;
            decimal prevHp2 = i >= 2 ? hpMa2List.ElementAtOrDefault(i - 2) : 0;

            var signal = GetCompareSignal(hp - prevHp1, prevHp1 - prevHp2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ehpf", hpMa2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = hpMa2List;
        stockData.IndicatorName = IndicatorName.EhlersHighPassFilterV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Decycler Oscillator V2
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="fastLength"></param>
    /// <param name="slowLength"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersDecyclerOscillatorV2(this StockData stockData, MovingAvgType maType = MovingAvgType.WeightedMovingAverage,
        int fastLength = 10, int slowLength = 20)
    {
        List<decimal> decList = new();
        List<Signal> signalsList = new();

        var hp1List = CalculateEhlersHighPassFilterV2(stockData, maType, fastLength).CustomValuesList;
        var hp2List = CalculateEhlersHighPassFilterV2(stockData, maType, slowLength).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal hp1 = hp1List.ElementAtOrDefault(i);
            decimal hp2 = hp2List.ElementAtOrDefault(i);

            decimal prevDec = decList.LastOrDefault();
            decimal dec = hp2 - hp1;
            decList.Add(dec);

            var signal = GetCompareSignal(dec, prevDec);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Edo", decList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = decList;
        stockData.IndicatorName = IndicatorName.EhlersDecyclerOscillatorV2;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Modified Stochastic Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersModifiedStochasticIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.Ehlers2PoleSuperSmootherFilterV1,
        int length1 = 48, int length2 = 10, int length3 = 20)
    {
        List<decimal> stocList = new();
        List<decimal> modStocList = new();
        List<Signal> signalsList = new();

        decimal a1 = Exp(-1.414m * Pi / length1);
        decimal b1 = 2 * a1 * Cos(Math.Min(1.414m * Pi / length1, 0.99m));
        decimal c2 = b1;
        decimal c3 = -1 * a1 * a1;
        decimal c1 = 1 - c2 - c3;

        var roofingFilterList = CalculateEhlersRoofingFilterV1(stockData, maType, length1, length2).CustomValuesList;
        var (highestList, lowestList) = GetMaxAndMinValuesList(roofingFilterList, length3);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highest = highestList.ElementAtOrDefault(i);
            decimal lowest = lowestList.ElementAtOrDefault(i);
            decimal roofingFilter = roofingFilterList.ElementAtOrDefault(i);
            decimal prevModStoc1 = i >= 1 ? modStocList.ElementAtOrDefault(i - 1) : 0;
            decimal prevModStoc2 = i >= 2 ? modStocList.ElementAtOrDefault(i - 2) : 0;

            decimal prevStoc = stocList.LastOrDefault();
            decimal stoc = highest - lowest != 0 ? (roofingFilter - lowest) / (highest - lowest) * 100 : 0;
            stocList.Add(stoc);

            decimal modStoc = (c1 * ((stoc + prevStoc) / 2)) + (c2 * prevModStoc1) + (c3 * prevModStoc2);
            modStocList.Add(modStoc);

            var signal = GetRsiSignal(modStoc - prevModStoc1, prevModStoc1 - prevModStoc2, modStoc, prevModStoc1, 70, 30);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Emsi", modStocList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = modStocList;
        stockData.IndicatorName = IndicatorName.EhlersModifiedStochasticIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Modified Relative Strength Index
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="length3"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersModifiedRelativeStrengthIndex(this StockData stockData, int length1 = 48, int length2 = 10, int length3 = 10)
    {
        List<decimal> upChgList = new();
        List<decimal> upChgSumList = new();
        List<decimal> denomList = new();
        List<decimal> mrsiList = new();
        List<decimal> mrsiSigList = new();
        List<Signal> signalsList = new();

        decimal a1 = Exp(-1.414m * Pi / length2);
        decimal b1 = 2 * a1 * Cos(Math.Min(1.414m * Pi / length2, 0.99m));
        decimal c2 = b1;
        decimal c3 = -1 * a1 * a1;
        decimal c1 = 1 - c2 - c3;

        var roofingFilterList = CalculateEhlersRoofingFilterV2(stockData, length1, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal roofingFilter = roofingFilterList.ElementAtOrDefault(i);
            decimal prevRoofingFilter = i >= 1 ? roofingFilterList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMrsi1 = i >= 1 ? mrsiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMrsi2 = i >= 2 ? mrsiList.ElementAtOrDefault(i - 2) : 0;
            decimal prevMrsiSig1 = i >= 1 ? mrsiSigList.ElementAtOrDefault(i - 1) : 0;
            decimal prevMrsiSig2 = i >= 2 ? mrsiSigList.ElementAtOrDefault(i - 2) : 0;

            decimal upChg = roofingFilter > prevRoofingFilter ? roofingFilter - prevRoofingFilter : 0;
            upChgList.Add(upChg);

            decimal dnChg = roofingFilter < prevRoofingFilter ? prevRoofingFilter - roofingFilter : 0;
            decimal prevUpChgSum = upChgSumList.LastOrDefault();
            decimal upChgSum = upChgList.TakeLastExt(length3).Sum();
            upChgSumList.Add(upChgSum);

            decimal prevDenom = denomList.LastOrDefault();
            decimal denom = upChg + dnChg;
            denomList.Add(denom);

            decimal mrsi = denom != 0 && prevDenom != 0 ? (c1 * (((upChgSum / denom) + (prevUpChgSum / prevDenom)) / 2)) + (c2 * prevMrsi1) + (c3 * prevMrsi2) : 0;
            mrsiList.Add(mrsi);

            decimal mrsiSig = (c1 * ((mrsi + prevMrsi1) / 2)) + (c2 * prevMrsiSig1) + (c3 * prevMrsiSig2);
            mrsiSigList.Add(mrsiSig);

            var signal = GetRsiSignal(mrsi - mrsiSig, prevMrsi1 - prevMrsiSig1, mrsi, prevMrsi1, 0.7m, 0.3m);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Emrsi", mrsiList },
            { "Signal", mrsiSigList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = mrsiList;
        stockData.IndicatorName = IndicatorName.EhlersModifiedRelativeStrengthIndex;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Hp Lp Roofing Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersHpLpRoofingFilter(this StockData stockData, int length1 = 48, int length2 = 10)
    {
        List<decimal> highPassList = new();
        List<decimal> roofingFilterList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alphaArg = Math.Min(2 * Pi / length1, 0.99m);
        decimal alphaCos = Cos(alphaArg);
        decimal alpha1 = alphaCos != 0 ? (alphaCos + Sin(alphaArg) - 1) / alphaCos : 0;
        decimal a1 = Exp(-1.414m * Pi / length2);
        decimal b1 = 2 * a1 * Cos(Math.Min(1.414m * Pi / length2, 0.99m));
        decimal c2 = b1;
        decimal c3 = -1 * a1 * a1;
        decimal c1 = 1 - c2 - c3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFilter1 = i >= 1 ? roofingFilterList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFilter2 = i >= 2 ? roofingFilterList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHp1 = i >= 1 ? highPassList.ElementAtOrDefault(i - 1) : 0;

            decimal hp = ((1 - (alpha1 / 2)) * (currentValue - prevValue1)) + ((1 - alpha1) * prevHp1);
            highPassList.Add(hp);

            decimal filter = (c1 * ((hp + prevHp1) / 2)) + (c2 * prevFilter1) + (c3 * prevFilter2);
            roofingFilterList.Add(filter);

            var signal = GetCompareSignal(filter - prevFilter1, prevFilter1 - prevFilter2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ehplprf", roofingFilterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = roofingFilterList;
        stockData.IndicatorName = IndicatorName.EhlersHpLpRoofingFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Decycler
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersDecycler(this StockData stockData, int length = 60)
    {
        List<decimal> decList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alphaArg = Math.Min(2 * Pi / length, 0.99m);
        decimal alphaCos = Cos(alphaArg);
        decimal alpha1 = alphaCos != 0 ? (alphaCos + Sin(alphaArg) - 1) / alphaCos : 0;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue1 = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;

            decimal prevDec = decList.LastOrDefault();
            decimal dec = (alpha1 / 2 * (currentValue + prevValue1)) + ((1 - alpha1) * prevDec);
            decList.Add(dec);

            var signal = GetCompareSignal(currentValue - dec, prevValue1 - prevDec);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ed", decList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = decList;
        stockData.IndicatorName = IndicatorName.EhlersDecycler;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Zero Mean Roofing Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersZeroMeanRoofingFilter(this StockData stockData, int length1 = 48, int length2 = 10)
    {
        List<decimal> zmrFilterList = new();
        List<Signal> signalsList = new();

        decimal alphaArg = Math.Min(2 * Pi / length1, 0.99m);
        decimal alphaCos = Cos(alphaArg);
        decimal alpha1 = alphaCos != 0 ? (alphaCos + Sin(alphaArg) - 1) / alphaCos : 0;

        var roofingFilterList = CalculateEhlersHpLpRoofingFilter(stockData, length1, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentRf = roofingFilterList.ElementAtOrDefault(i);
            decimal prevRf = i >= 1 ? roofingFilterList.ElementAtOrDefault(i - 1) : 0;
            decimal prevZmrFilt1 = i >= 1 ? zmrFilterList.ElementAtOrDefault(i - 1) : 0;
            decimal prevZmrFilt2 = i >= 2 ? zmrFilterList.ElementAtOrDefault(i - 2) : 0;

            decimal zmrFilt = ((1 - (alpha1 / 2)) * (currentRf - prevRf)) + ((1 - alpha1) * prevZmrFilt1);
            zmrFilterList.Add(zmrFilt);

            var signal = GetCompareSignal(zmrFilt - prevZmrFilt1, prevZmrFilt1 - prevZmrFilt2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ezmrf", zmrFilterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = zmrFilterList;
        stockData.IndicatorName = IndicatorName.EhlersZeroMeanRoofingFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Roofing Filter Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersRoofingFilterIndicator(this StockData stockData, int length1 = 80, int length2 = 40)
    {
        List<decimal> highPassList = new();
        List<decimal> roofingFilterList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal alphaArg = Math.Min(0.707m * 2 * Pi / length1, 0.99m);
        decimal alphaCos = Cos(alphaArg);
        decimal a1 = alphaCos != 0 ? (alphaCos + Sin(alphaArg) - 1) / alphaCos : 0;
        decimal a2 = Exp(-1.414m * Pi / length2);
        decimal b1 = 2 * a2 * Cos(Math.Min(1.414m * Pi / length2, 0.99m));
        decimal c2 = b1;
        decimal c3 = -a2 * a2;
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

            decimal hp = (Pow(1 - (a1 / 2), 2) * (currentValue - (2 * prevValue1) + prevValue2)) + (2 * (1 - a1) * prevHp1) - (Pow(1 - a1, 2) * prevHp2);
            highPassList.Add(hp);

            decimal filter = (c1 * ((hp + prevHp1) / 2)) + (c2 * prevFilter1) + (c3 * prevFilter2);
            roofingFilterList.Add(filter);

            var signal = GetCompareSignal(filter - prevFilter1, prevFilter1 - prevFilter2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Erfi", roofingFilterList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = roofingFilterList;
        stockData.IndicatorName = IndicatorName.EhlersRoofingFilterIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Hurst Coefficient
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersHurstCoefficient(this StockData stockData, int length1 = 30, int length2 = 20)
    {
        List<decimal> dimenList = new();
        List<decimal> hurstList = new();
        List<decimal> smoothHurstList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        int hLength = (int)Math.Ceiling((decimal)length1 / 2);
        decimal a1 = Exp(-1.414m * Pi / length2);
        decimal b1 = 2 * a1 * Cos(Math.Min(1.414m * Pi / length2, 0.99m));
        decimal c2 = b1;
        decimal c3 = -a1 * a1;
        decimal c1 = 1 - c2 - c3;

        var (hh3List, ll3List) = GetMaxAndMinValuesList(inputList, length1);
        var (hh1List, ll1List) = GetMaxAndMinValuesList(inputList, hLength);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal hh3 = hh3List.ElementAtOrDefault(i);
            decimal ll3 = ll3List.ElementAtOrDefault(i);
            decimal hh1 = hh1List.ElementAtOrDefault(i);
            decimal ll1 = ll1List.ElementAtOrDefault(i);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal priorValue = i >= hLength ? inputList.ElementAtOrDefault(i - hLength) : currentValue;
            decimal prevSmoothHurst1 = i >= 1 ? smoothHurstList.ElementAtOrDefault(i - 1) : 0;
            decimal prevSmoothHurst2 = i >= 2 ? smoothHurstList.ElementAtOrDefault(i - 2) : 0;
            decimal n3 = (hh3 - ll3) / length1;
            decimal n1 = (hh1 - ll1) / hLength;
            decimal hh2 = i >= hLength ? priorValue : currentValue;
            decimal ll2 = i >= hLength ? priorValue : currentValue;

            for (int j = hLength; j < length1; j++)
            {
                decimal price = i >= j ? inputList.ElementAtOrDefault(i - j) : 0;
                hh2 = price > hh2 ? price : hh2;
                ll2 = price < ll2 ? price : ll2;
            }
            decimal n2 = (hh2 - ll2) / hLength;

            decimal prevDimen = dimenList.LastOrDefault();
            decimal dimen = 0.5m * (((Log(n1 + n2) - Log(n3)) / Log(2)) + prevDimen);
            dimenList.Add(dimen);

            decimal prevHurst = hurstList.LastOrDefault();
            decimal hurst = 2 - dimen;
            hurstList.Add(hurst);

            decimal smoothHurst = (c1 * ((hurst + prevHurst) / 2)) + (c2 * prevSmoothHurst1) + (c3 * prevSmoothHurst2);
            smoothHurstList.Add(smoothHurst);

            var signal = GetCompareSignal(smoothHurst - prevSmoothHurst1, prevSmoothHurst1 - prevSmoothHurst2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Ehc", smoothHurstList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = smoothHurstList;
        stockData.IndicatorName = IndicatorName.EhlersHurstCoefficient;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Reflex Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersReflexIndicator(this StockData stockData, int length = 20)
    {
        List<decimal> filterList = new();
        List<decimal> msList = new();
        List<decimal> reflexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(-1.414m * Pi / 0.5m * length);
        decimal b1 = 2 * a1 * Cos(1.414m * Pi / 0.5m * length);
        decimal c2 = b1;
        decimal c3 = -a1 * a1;
        decimal c1 = 1 - c2 - c3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFilter1 = filterList.LastOrDefault();
            decimal prevFilter2 = i >= 2 ? filterList.ElementAtOrDefault(i - 2) : 0;
            decimal priorFilter = i >= length ? filterList.ElementAtOrDefault(i - length) : 0;
            decimal prevReflex1 = i >= 1 ? reflexList.ElementAtOrDefault(i - 1) : 0;
            decimal prevReflex2 = i >= 2 ? reflexList.ElementAtOrDefault(i - 2) : 0;

            decimal filter = (c1 * ((currentValue + prevValue) / 2)) + (c2 * prevFilter1) + (c3 * prevFilter2);
            filterList.Add(filter);

            decimal slope = length != 0 ? (priorFilter - filter) / length : 0;
            decimal sum = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevFilterCount = i >= j ? filterList.ElementAtOrDefault(i - j) : 0;
                sum += filter + (j * slope) - prevFilterCount;
            }
            sum /= length;

            decimal prevMs = msList.LastOrDefault();
            decimal ms = (0.04m * sum * sum) + (0.96m * prevMs);
            msList.Add(ms);

            decimal reflex = ms > 0 ? sum / Sqrt(ms) : 0;
            reflexList.Add(reflex);

            var signal = GetCompareSignal(reflex - prevReflex1, prevReflex1 - prevReflex2);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eri", reflexList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = reflexList;
        stockData.IndicatorName = IndicatorName.EhlersReflexIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Spectrum Derived Filter Bank
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersSpectrumDerivedFilterBank(this StockData stockData, int minLength = 8, int maxLength = 50, 
        int length1 = 40, int length2 = 10)
    {
        List<decimal> dcList = new();
        List<decimal> domCycList = new();
        List<decimal> realList = new();
        List<decimal> imagList = new();
        List<decimal> q1List = new();
        List<decimal> hpList = new();
        List<decimal> smoothHpList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal twoPiPer = MinOrMax(2 * Pi / length1, 0.99m, 0.01m);
        decimal alpha1 = (1 - Sin(twoPiPer)) / Cos(twoPiPer);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal delta = Math.Max((-0.015m * i) + 0.5m, 0.15m);
            decimal prevHp1 = i >= 1 ? hpList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHp2 = i >= 2 ? hpList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHp3 = i >= 3 ? hpList.ElementAtOrDefault(i - 3) : 0;
            decimal prevHp4 = i >= 4 ? hpList.ElementAtOrDefault(i - 4) : 0;
            decimal prevHp5 = i >= 5 ? hpList.ElementAtOrDefault(i - 5) : 0;

            decimal hp = i < 7 ? currentValue : (0.5m * (1 + alpha1) * (currentValue - prevValue)) + (alpha1 * prevHp1);
            hpList.Add(hp);

            decimal prevSmoothHp = smoothHpList.LastOrDefault();
            decimal smoothHp = i < 7 ? currentValue - prevValue : (hp + (2 * prevHp1) + (3 * prevHp2) + (3 * prevHp3) + (2 * prevHp4) + prevHp5) / 12;
            smoothHpList.Add(smoothHp);

            decimal num = 0, denom = 0, dc = 0, real = 0, imag = 0, q1 = 0, maxAmpl = 0;
            for (int j = minLength; j <= maxLength; j++)
            {
                decimal beta = Cos(MinOrMax(2 * Pi / j, 0.99m, 0.01m));
                decimal gamma = 1 / Cos(MinOrMax(4 * Pi * delta / j, 0.99m, 0.01m));
                decimal alpha = gamma - Sqrt((gamma * gamma) - 1);
                decimal priorSmoothHp = i >= j ? smoothHpList.ElementAtOrDefault(i - j) : 0;
                decimal prevReal = i >= j ? realList.ElementAtOrDefault(i - j) : 0;
                decimal priorReal = i >= j * 2 ? realList.ElementAtOrDefault(i - (j * 2)) : 0;
                decimal prevImag = i >= j ? imagList.ElementAtOrDefault(i - j) : 0;
                decimal priorImag = i >= j * 2 ? imagList.ElementAtOrDefault(i - (j * 2)) : 0;
                decimal prevQ1 = i >= j ? q1List.ElementAtOrDefault(i - j) : 0;

                q1 = j / Pi * 2 * (smoothHp - prevSmoothHp);
                real = (0.5m * (1 - alpha) * (smoothHp - priorSmoothHp)) + (beta * (1 + alpha) * prevReal) - (alpha * priorReal);
                imag = (0.5m * (1 - alpha) * (q1 - prevQ1)) + (beta * (1 + alpha) * prevImag) - (alpha * priorImag);
                decimal ampl = (real * real) + (imag * imag);
                maxAmpl = ampl > maxAmpl ? ampl : maxAmpl;
                decimal db = maxAmpl != 0 && ampl / maxAmpl > 0 ? -length2 * Log(0.01m / (1 - (0.99m * ampl / maxAmpl))) / Log(length2) : 0;
                db = db > maxLength ? maxLength : db;
                num += db <= 3 ? j * (maxLength - db) : 0;
                denom += db <= 3 ? maxLength - db : 0;
                dc = denom != 0 ? num / denom : 0;
            }
            q1List.Add(q1);
            realList.Add(real);
            imagList.Add(imag);
            dcList.Add(dc);

            decimal domCyc = dcList.TakeLastExt(length2).Median();
            domCycList.Add(domCyc);

            var signal = GetCompareSignal(smoothHp, prevSmoothHp);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Esdfb", domCycList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = domCycList;
        stockData.IndicatorName = IndicatorName.EhlersSpectrumDerivedFilterBank;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Dominant Cycle Tuned Bypass Filter
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersDominantCycleTunedBypassFilter(this StockData stockData, int minLength = 8, int maxLength = 50, 
        int length1 = 40, int length2 = 10)
    {
        List<decimal> v1List = new();
        List<decimal> v2List = new();
        List<decimal> hpList = new();
        List<decimal> smoothHpList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal twoPiPer = MinOrMax(2 * Pi / length1, 0.99m, 0.01m);
        decimal alpha1 = (1 - Sin(twoPiPer)) / Cos(twoPiPer);

        var domCycList = CalculateEhlersSpectrumDerivedFilterBank(stockData, minLength, maxLength, length1, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal domCyc = domCycList.ElementAtOrDefault(i);
            decimal beta = Cos(MinOrMax(2 * Pi / domCyc, 0.99m, 0.01m));
            decimal delta = Math.Max((-0.015m * i) + 0.5m, 0.15m);
            decimal gamma = 1 / Cos(MinOrMax(4 * Pi * (delta / domCyc), 0.99m, 0.01m));
            decimal alpha = gamma - Sqrt((gamma * gamma) - 1);
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHp1 = i >= 1 ? hpList.ElementAtOrDefault(i - 1) : 0;
            decimal prevHp2 = i >= 2 ? hpList.ElementAtOrDefault(i - 2) : 0;
            decimal prevHp3 = i >= 3 ? hpList.ElementAtOrDefault(i - 3) : 0;
            decimal prevHp4 = i >= 4 ? hpList.ElementAtOrDefault(i - 4) : 0;
            decimal prevHp5 = i >= 5 ? hpList.ElementAtOrDefault(i - 5) : 0;

            decimal hp = i < 7 ? currentValue : (0.5m * (1 + alpha1) * (currentValue - prevValue)) + (alpha1 * prevHp1);
            hpList.Add(hp);

            decimal prevSmoothHp = smoothHpList.LastOrDefault();
            decimal smoothHp = i < 7 ? currentValue - prevValue : (hp + (2 * prevHp1) + (3 * prevHp2) + (3 * prevHp3) + (2 * prevHp4) + prevHp5) / 12;
            smoothHpList.Add(smoothHp);

            decimal prevV1 = i >= 1 ? v1List.ElementAtOrDefault(i - 1) : 0;
            decimal prevV1_2 = i >= 2 ? v1List.ElementAtOrDefault(i - 2) : 0;
            decimal v1 = (0.5m * (1 - alpha) * (smoothHp - prevSmoothHp)) + (beta * (1 + alpha) * prevV1) - (alpha * prevV1_2);
            v1List.Add(v1);

            decimal v2 = domCyc / Pi * 2 * (v1 - prevV1);
            v2List.Add(v2);

            var signal = GetConditionSignal(v2 > v1 && v2 >= 0, v2 < v1 || v2 < 0);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "V1", v1List },
            { "V2", v2List }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.EhlersDominantCycleTunedBypassFilter;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Restoring Pull Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersRestoringPullIndicator(this StockData stockData, MovingAvgType maType = MovingAvgType.ExponentialMovingAverage,
        int minLength = 8, int maxLength = 50, int length1 = 40, int length2 = 10)
    {
        List<decimal> rpiList = new();
        List<Signal> signalsList = new();
        var (_, _, _, _, volumeList) = GetInputValuesList(stockData);

        var domCycList = CalculateEhlersSpectrumDerivedFilterBank(stockData, minLength, maxLength, length1, length2).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal domCyc = domCycList.ElementAtOrDefault(i);
            decimal volume = volumeList.ElementAtOrDefault(i);

            decimal rpi = volume * Pow(MinOrMax(2 * Pi / domCyc, 0.99m, 0.01m), 2);
            rpiList.Add(rpi);
        }

        var rpiEmaList = GetMovingAverageList(stockData, maType, minLength, rpiList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal rpi = rpiList.ElementAtOrDefault(i);
            decimal rpiEma = rpiEmaList.ElementAtOrDefault(i);
            decimal prevRpi = i >= 1 ? rpiList.ElementAtOrDefault(i - 1) : 0;
            decimal prevRpiEma = i >= 1 ? rpiEmaList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(rpi - rpiEma, prevRpi - prevRpiEma, true);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Rpi", rpiList },
            { "Signal", rpiEmaList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = rpiList;
        stockData.IndicatorName = IndicatorName.EhlersRestoringPullIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Trendflex Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersTrendflexIndicator(this StockData stockData, int length = 20)
    {
        List<decimal> filterList = new();
        List<decimal> msList = new();
        List<decimal> trendflexList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal a1 = Exp(-1.414m * Pi / 0.5m * length);
        decimal b1 = 2 * a1 * Cos(1.414m * Pi / 0.5m * length);
        decimal c2 = b1;
        decimal c3 = -a1 * a1;
        decimal c1 = 1 - c2 - c3;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue = i >= 1 ? inputList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFilter1 = i >= 1 ? filterList.ElementAtOrDefault(i - 1) : 0;
            decimal prevFilter2 = i >= 2 ? filterList.ElementAtOrDefault(i - 2) : 0;

            decimal filter = (c1 * ((currentValue + prevValue) / 2)) + (c2 * prevFilter1) + (c3 * prevFilter2);
            filterList.Add(filter);

            decimal sum = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal prevFilterCount = i >= j ? filterList.ElementAtOrDefault(i - j) : 0;
                sum += filter - prevFilterCount;
            }
            sum /= length;

            decimal prevMs = msList.LastOrDefault();
            decimal ms = (0.04m * Pow(sum, 2)) + (0.96m * prevMs);
            msList.Add(ms);

            decimal prevTrendflex = trendflexList.LastOrDefault();
            decimal trendflex = ms > 0 ? sum / Sqrt(ms) : 0;
            trendflexList.Add(trendflex);

            var signal = GetCompareSignal(trendflex, prevTrendflex);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eti", trendflexList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trendflexList;
        stockData.IndicatorName = IndicatorName.EhlersTrendflexIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Correlation Cycle Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersCorrelationCycleIndicator(this StockData stockData, int length = 20)
    {
        List<decimal> realList = new();
        List<decimal> imagList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal sx = 0, sy = 0, nsy = 0, sxx = 0, syy = 0, nsyy = 0, sxy = 0, nsxy = 0;
            for (int j = 1; j <= length; j++)
            {
                decimal x = i >= j - 1 ? inputList.ElementAtOrDefault(i - (j - 1)) : 0;
                decimal v = MinOrMax(2 * Pi * ((decimal)(j - 1) / length), 0.99m, 0.01m);
                decimal y = Cos(v);
                decimal ny = -Sin(v);
                sx += x;
                sy += y;
                nsy += ny;
                sxx += Pow(x, 2);
                syy += Pow(y, 2);
                nsyy += ny * ny;
                sxy += x * y;
                nsxy += x * ny;
            }

            decimal prevReal = realList.LastOrDefault();
            decimal real = (length * sxx) - (sx * sx) > 0 && (length * syy) - (sy * sy) > 0 ? ((length * sxy) - (sx * sy)) /
                   Sqrt(((length * sxx) - (sx * sx)) * ((length * syy) - (sy * sy))) : 0;
            realList.Add(real);

            decimal prevImag = imagList.LastOrDefault();
            decimal imag = (length * sxx) - (sx * sx) > 0 && (length * nsyy) - (nsy * nsy) > 0 ? ((length * nsxy) - (sx * nsy)) /
                   Sqrt(((length * sxx) - (sx * sx)) * ((length * nsyy) - (nsy * nsy))) : 0;
            imagList.Add(imag);

            var signal = GetCompareSignal(real - imag, prevReal - prevImag);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Real", realList },
            { "Imag", imagList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.EhlersCorrelationCycleIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Correlation Angle Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersCorrelationAngleIndicator(this StockData stockData, int length = 20)
    {
        List<decimal> angleList = new();
        List<Signal> signalsList = new();

        var ecciList = CalculateEhlersCorrelationCycleIndicator(stockData, length);
        var realList = ecciList.OutputValues["Real"];
        var imagList = ecciList.OutputValues["Imag"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal real = realList.ElementAtOrDefault(i);
            decimal imag = imagList.ElementAtOrDefault(i);

            decimal prevAngle = i >= 1 ? angleList.ElementAtOrDefault(i - 1) : 0;
            decimal angle = imag != 0 ? 90 + Atan(real / imag).ToDegrees() : 90;
            angle = imag > 0 ? angle - 180 : angle;
            angle = prevAngle - angle < 270 && angle < prevAngle ? prevAngle : angle;
            angleList.Add(angle);

            var signal = GetCompareSignal(angle, prevAngle);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Cai", angleList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = angleList;
        stockData.IndicatorName = IndicatorName.EhlersCorrelationAngleIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Market State Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersMarketStateIndicator(this StockData stockData, int length = 20)
    {
        List<decimal> stateList = new();
        List<Signal> signalsList = new();

        var angleList = CalculateEhlersCorrelationAngleIndicator(stockData, length).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal angle = angleList.ElementAtOrDefault(i);
            decimal prevAngle = i >= 1 ? angleList.ElementAtOrDefault(i - 1) : 0;

            decimal prevState = stateList.LastOrDefault();
            decimal state = Math.Abs(angle - prevAngle) < 9 && angle < 0 ? -1 : Math.Abs(angle - prevAngle) < 9 && angle >= 0 ? 1 : 0;
            stateList.Add(state);

            var signal = GetCompareSignal(state, prevState);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Emsi", stateList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = stateList;
        stockData.IndicatorName = IndicatorName.EhlersMarketStateIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Trend Extraction
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersTrendExtraction(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length = 20, decimal delta = 0.1m)
    {
        List<decimal> bpList = new();
        List<Signal> signalsList = new();
        var (inputList, _, _, _, _) = GetInputValuesList(stockData);

        decimal beta = Math.Max(Cos(2 * Pi / length), 0.99m);
        decimal gamma = 1 / Cos(4 * Pi * delta / length);
        decimal alpha = Math.Max(gamma - Sqrt((gamma * gamma) - 1), 0.99m);

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal currentValue = inputList.ElementAtOrDefault(i);
            decimal prevValue2 = i >= 2 ? inputList.ElementAtOrDefault(i - 2) : 0;
            decimal prevBp1 = i >= 1 ? bpList.ElementAtOrDefault(i - 1) : 0;
            decimal prevBp2 = i >= 2 ? bpList.ElementAtOrDefault(i - 2) : 0;

            decimal bp = (0.5m * (1 - alpha) * (currentValue - prevValue2)) + (beta * (1 + alpha) * prevBp1) - (alpha * prevBp2);
            bpList.Add(bp);
        }

        var trendList = GetMovingAverageList(stockData, maType, length * 2, bpList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal trend = trendList.ElementAtOrDefault(i);
            decimal prevTrend = i >= 1 ? trendList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(trend, prevTrend);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Trend", trendList },
            { "Bp", bpList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = trendList;
        stockData.IndicatorName = IndicatorName.EhlersTrendExtraction;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Empirical Mode Decomposition
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="delta"></param>
    /// <param name="fraction"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersEmpiricalModeDecomposition(this StockData stockData, MovingAvgType maType = MovingAvgType.SimpleMovingAverage,
        int length1 = 20, int length2 = 50, decimal delta = 0.5m, decimal fraction = 0.1m)
    {
        List<decimal> peakList = new();
        List<decimal> valleyList = new();
        List<decimal> peakAvgFracList = new();
        List<decimal> valleyAvgFracList = new();
        List<Signal> signalsList = new();

        var eteList = CalculateEhlersTrendExtraction(stockData, maType, length1, delta);
        var trendList = eteList.OutputValues["Trend"];
        var bpList = eteList.OutputValues["Bp"];

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal prevBp1 = i >= 1 ? bpList.ElementAtOrDefault(i - 1) : 0;
            decimal prevBp2 = i >= 2 ? bpList.ElementAtOrDefault(i - 2) : 0;
            decimal bp = bpList.ElementAtOrDefault(i);

            decimal prevPeak = peakList.LastOrDefault();
            decimal peak = prevBp1 > bp && prevBp1 > prevBp2 ? prevBp1 : prevPeak;
            peakList.Add(peak);

            decimal prevValley = valleyList.LastOrDefault();
            decimal valley = prevBp1 < bp && prevBp1 < prevBp2 ? prevBp1 : prevValley;
            valleyList.Add(valley);
        }

        var peakAvgList = GetMovingAverageList(stockData, maType, length2, peakList);
        var valleyAvgList = GetMovingAverageList(stockData, maType, length2, valleyList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal peakAvg = peakAvgList.ElementAtOrDefault(i);
            decimal valleyAvg = valleyAvgList.ElementAtOrDefault(i);
            decimal trend = trendList.ElementAtOrDefault(i);
            decimal prevTrend = i >= 1 ? trendList.ElementAtOrDefault(i - 1) : 0;

            decimal prevPeakAvgFrac = peakAvgFracList.LastOrDefault();
            decimal peakAvgFrac = fraction * peakAvg;
            peakAvgFracList.Add(peakAvgFrac);

            decimal prevValleyAvgFrac = valleyAvgFracList.LastOrDefault();
            decimal valleyAvgFrac = fraction * valleyAvg;
            valleyAvgFracList.Add(valleyAvgFrac);

            var signal = GetBullishBearishSignal(trend - Math.Max(peakAvgFrac, valleyAvgFrac), prevTrend - Math.Max(prevPeakAvgFrac, prevValleyAvgFrac),
                trend - Math.Min(peakAvgFrac, valleyAvgFrac), prevTrend - Math.Min(prevPeakAvgFrac, prevValleyAvgFrac));
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Trend", trendList },
            { "Peak", peakAvgFracList },
            { "Valley", valleyAvgFracList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = new List<decimal>();
        stockData.IndicatorName = IndicatorName.EhlersEmpiricalModeDecomposition;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Early Onset Trend Indicator
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersEarlyOnsetTrendIndicator(this StockData stockData, int length1 = 30, int length2 = 100, decimal k = 0.85m)
    {
        List<decimal> peakList = new();
        List<decimal> quotientList = new();
        List<Signal> signalsList = new();

        var hpList = CalculateEhlersHighPassFilterV1(stockData, length2, 1).CustomValuesList;
        stockData.CustomValuesList = hpList;
        var superSmoothList = CalculateEhlersSuperSmootherFilter(stockData, length1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal filter = superSmoothList.ElementAtOrDefault(i);

            decimal prevPeak = peakList.LastOrDefault();
            decimal peak = Math.Abs(filter) > 0.991m * prevPeak ? Math.Abs(filter) : 0.991m * prevPeak;
            peakList.Add(peak);

            decimal ratio = peak != 0 ? filter / peak : 0;
            decimal prevQuotient = quotientList.LastOrDefault();
            decimal quotient = (k * ratio) + 1 != 0 ? (ratio + k) / ((k * ratio) + 1) : 0;
            quotientList.Add(quotient);

            var signal = GetCompareSignal(quotient, prevQuotient);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Eoti", quotientList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = quotientList;
        stockData.IndicatorName = IndicatorName.EhlersEarlyOnsetTrendIndicator;

        return stockData;
    }

    /// <summary>
    /// Calculates the Ehlers Roofing Filter V1
    /// </summary>
    /// <param name="stockData"></param>
    /// <param name="maType"></param>
    /// <param name="length1"></param>
    /// <param name="length2"></param>
    /// <returns></returns>
    public static StockData CalculateEhlersRoofingFilterV1(this StockData stockData, MovingAvgType maType = MovingAvgType.Ehlers2PoleSuperSmootherFilterV1, 
        int length1 = 48, int length2 = 10)
    {
        List<decimal> argList = new();
        List<Signal> signalsList = new();

        var hpFilterList = CalculateEhlersHighPassFilterV1(stockData, length1, 1).CustomValuesList;

        for (int i = 0; i < stockData.Count; i++)
        {
            decimal highPass = hpFilterList.ElementAtOrDefault(i);
            decimal prevHp1 = i >= 1 ? hpFilterList.ElementAtOrDefault(i - 1) : 0;

            decimal arg = (highPass + prevHp1) / 2;
            argList.Add(arg);
        }

        var roofingFilter2PoleList = GetMovingAverageList(stockData, maType, length2, argList);
        for (int i = 0; i < stockData.Count; i++)
        {
            decimal roofingFilter = roofingFilter2PoleList.ElementAtOrDefault(i);
            decimal prevRoofingFilter = i >= 1 ? roofingFilter2PoleList.ElementAtOrDefault(i - 1) : 0;

            var signal = GetCompareSignal(roofingFilter, prevRoofingFilter);
            signalsList.Add(signal);
        }

        stockData.OutputValues = new()
        {
            { "Erf", roofingFilter2PoleList }
        };
        stockData.SignalsList = signalsList;
        stockData.CustomValuesList = roofingFilter2PoleList;
        stockData.IndicatorName = IndicatorName.EhlersRoofingFilterV1;

        return stockData;
    }
}