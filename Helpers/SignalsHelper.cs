//     Ooples Finance Stock Indicator Library
//     https://ooples.github.io/OoplesFinance.StockIndicators/
//
//     Copyright © Franklin Moormann, 2020-2022
//     cheatcountry@gmail.com
//
//     This library is free software and it uses the Apache 2.0 license
//     so if you are going to re-use or modify my code then I just ask
//     that you include my copyright info and my contact info in a comment

namespace OoplesFinance.StockIndicators.Helpers;

public static class SignalHelper
{
    /// <summary>
    /// Gets the rsi signal.
    /// </summary>
    /// <param name="currentSlope">The current slope.</param>
    /// <param name="prevSlope">The previous slope.</param>
    /// <param name="currentRsi">The current rsi.</param>
    /// <param name="prevRsi">The previous rsi.</param>
    /// <param name="overBoughtNumber">The over bought number.</param>
    /// <param name="overSoldNumber">The over sold number.</param>
    /// <param name="isReversed">if set to <c>true</c> [is reversed].</param>
    /// <returns></returns>
    public static Signal GetRsiSignal(decimal currentSlope, decimal prevSlope, decimal currentRsi, decimal prevRsi, decimal overBoughtNumber, decimal overSoldNumber, bool isReversed = false)
    {
        Signal signal;

        if (isReversed ? currentSlope < 0 && currentSlope < prevSlope : currentSlope > 0 && currentSlope > prevSlope)
        {
            signal = Signal.StrongBuy;
        }
        else if (isReversed ? currentSlope > 0 && currentSlope > prevSlope : currentSlope < 0 && currentSlope < prevSlope)
        {
            signal = Signal.StrongSell;
        }
        else if (isReversed ? currentSlope < 0 || (prevRsi > overSoldNumber && currentRsi < overSoldNumber) : currentSlope > 0 || (prevRsi < overSoldNumber && currentRsi > overSoldNumber))
        {
            signal = Signal.Buy;
        }
        else if (isReversed ? currentSlope > 0 || (prevRsi < overBoughtNumber && currentRsi > overBoughtNumber) : currentSlope < 0 || (prevRsi > overBoughtNumber && currentRsi < overBoughtNumber))
        {
            signal = Signal.Sell;
        }
        else
        {
            signal = Signal.None;
        }

        return signal;
    }

    /// <summary>
    /// Gets the bullish bearish signal.
    /// </summary>
    /// <param name="bullishSlope">The bullish slope.</param>
    /// <param name="prevBullishSlope">The previous bullish slope.</param>
    /// <param name="bearishSlope">The bearish slope.</param>
    /// <param name="prevBearishSlope">The previous bearish slope.</param>
    /// <param name="isReversed">if set to <c>true</c> [is reversed].</param>
    /// <returns></returns>
    public static Signal GetBullishBearishSignal(decimal bullishSlope, decimal prevBullishSlope, decimal bearishSlope, decimal prevBearishSlope, bool isReversed = false)
    {
        Signal signal;

        if (isReversed ? bullishSlope < 0 && bullishSlope < prevBullishSlope : bullishSlope > 0 && bullishSlope > prevBullishSlope)
        {
            signal = Signal.StrongBuy;
        }
        else if (isReversed ? bearishSlope > 0 && bearishSlope > prevBearishSlope : bearishSlope < 0 && bearishSlope < prevBearishSlope)
        {
            signal = Signal.StrongSell;
        }
        else if (isReversed ? bullishSlope < 0 : bullishSlope > 0)
        {
            signal = Signal.Buy;
        }
        else if (isReversed ? bearishSlope > 0 : bearishSlope < 0)
        {
            signal = Signal.Sell;
        }
        else
        {
            signal = Signal.None;
        }

        return signal;
    }

    /// <summary>
    /// Gets the condition signal.
    /// </summary>
    /// <param name="bullishCond">if set to <c>true</c> [bullish cond].</param>
    /// <param name="bearishCond">if set to <c>true</c> [bearish cond].</param>
    /// <returns></returns>
    public static Signal GetConditionSignal(bool bullishCond, bool bearishCond)
    {
        Signal signal;

        if (bullishCond)
        {
            signal = Signal.Buy;
        }
        else if (bearishCond)
        {
            signal = Signal.Sell;
        }
        else
        {
            signal = Signal.None;
        }

        return signal;
    }

    /// <summary>
    /// Gets the compare signal.
    /// </summary>
    /// <param name="currentSlope">The current slope.</param>
    /// <param name="prevSlope">The previous slope.</param>
    /// <param name="isReversed">if set to <c>true</c> [is reversed].</param>
    /// <returns></returns>
    public static Signal GetCompareSignal(decimal currentSlope, decimal prevSlope, bool isReversed = false)
    {
        Signal signal;

        if (isReversed ? currentSlope < 0 && currentSlope < prevSlope : currentSlope > 0 && currentSlope > prevSlope)
        {
            signal = Signal.StrongBuy;
        }
        else if (isReversed ? currentSlope > 0 && currentSlope > prevSlope : currentSlope < 0 && currentSlope < prevSlope)
        {
            signal = Signal.StrongSell;
        }
        else if (isReversed ? currentSlope < 0 : currentSlope > 0)
        {
            signal = Signal.Buy;
        }
        else if (isReversed ? currentSlope > 0 : currentSlope < 0)
        {
            signal = Signal.Sell;
        }
        else
        {
            signal = Signal.None;
        }

        return signal;
    }

    /// <summary>
    /// Gets the bollinger bands signal.
    /// </summary>
    /// <param name="currentSlope">The current slope.</param>
    /// <param name="prevSlope">The previous slope.</param>
    /// <param name="currentValue">The current value.</param>
    /// <param name="prevValue">The previous value.</param>
    /// <param name="upperBand">The upper band.</param>
    /// <param name="prevUpperBand">The previous upper band.</param>
    /// <param name="lowerBand">The lower band.</param>
    /// <param name="prevLowerBand">The previous lower band.</param>
    /// <returns></returns>
    public static Signal GetBollingerBandsSignal(decimal currentSlope, decimal prevSlope, decimal currentValue, decimal prevValue, decimal upperBand, decimal prevUpperBand, decimal lowerBand, decimal prevLowerBand)
    {
        Signal signal;

        if (currentSlope > 0 && currentSlope > prevSlope)
        {
            signal = Signal.StrongBuy;
        }
        else if (currentSlope < 0 && currentSlope < prevSlope)
        {
            signal = Signal.StrongSell;
        }
        else if (currentSlope > 0 || (prevValue < prevLowerBand && currentValue > lowerBand))
        {
            signal = Signal.Buy;
        }
        else if (currentSlope < 0 || (prevValue > prevUpperBand && currentValue < upperBand))
        {
            signal = Signal.Sell;
        }
        else
        {
            signal = Signal.None;
        }

        return signal;
    }

    /// <summary>
    /// Gets the volatility signal.
    /// </summary>
    /// <param name="currentSlope">The current slope.</param>
    /// <param name="prevSlope">The previous slope.</param>
    /// <param name="currentVolatility">The current volatility.</param>
    /// <param name="thresholdValue">The threshold value.</param>
    /// <returns></returns>
    public static Signal GetVolatilitySignal(decimal currentSlope, decimal prevSlope, decimal currentVolatility, decimal thresholdValue)
    {
        Signal signal;

        if (currentVolatility >= thresholdValue)
        {
            if (currentSlope > 0 && currentSlope > prevSlope)
            {
                // if stock is in a buy signal
                signal = Signal.StrongBuy;
            }
            else if (currentSlope < 0 && currentSlope < prevSlope)
            {
                // if stock is in a sell signal
                signal = Signal.StrongSell;
            }
            else if (currentSlope > 0)
            {
                signal = Signal.Buy;
            }
            else if (currentSlope < 0)
            {
                signal = Signal.Sell;
            }
            else
            {
                signal = Signal.None;
            }
        }
        else
        {
            signal = Signal.None;
        }

        return signal;
    }
}