using static OoplesFinance.StockIndicators.Enums.SignalsClass;

namespace OoplesFinance.StockIndicators.Helpers
{
    public class SignalHelper
    {
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
}
