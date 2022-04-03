## .Net Stock Indicator Library

This is a stock indicator library that is completely open source and very easy to use. Current version contains [762 stock indicators](https://ooples.github.io/OoplesFinance.StockIndicators/indicators) and I will add more as I get requests for them!

### How to use this library

Here is an example to show how easy it is to create indicators using other indicators

```cs
var stockData = new StockData(openPrices, highPrices, lowPrices, closePrices, volumes);
var results = stockData.CalculateRelativeStrengthIndex().CalculateMovingAverageConvergenceDivergence();
```

Here is an example to get you started using the [Alpaca C# Api](https://github.com/alpacahq/alpaca-trade-api-csharp)

```cs
using Alpaca.Markets;
using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Calculations;

const string paperApiKey = "REPLACEME";
const string paperApiSecret = "REPLACEME";
const string symbol = "AAPL";
var startDate = new DateTime(2021, 01, 01);
var endDate = new DateTime(2021, 12, 31);

var client = Environments.Paper.GetAlpacaDataClient(new SecretKey(paperApiKey, paperApiSecret));
var bars = (await client.ListHistoricalBarsAsync(new HistoricalBarsRequest(symbol, startDate, endDate, BarTimeFrame.Day)).ConfigureAwait(false)).Items;
var stockData = new StockData(bars.Select(x => x.Open), bars.Select(x => x.High), bars.Select(x => x.Low), bars.Select(x => x.Close), bars.Select(x => x.Volume), bars.Select(x => x.TimeUtc));

// simple example showing default bollinger bands
var results = stockData.CalculateBollingerBands();
var upperBandList = results.OutputValues["UpperBand"];
var middleBandList = results.OutputValues["MiddleBand"];
var lowerBandList = results.OutputValues["LowerBand"];
```

```cs
// more advanced example showing bollinger bands with full customization and using a custom input of high rather than the default close
var stockData = new StockData(bars.Select(x => x.Open), bars.Select(x => x.High), bars.Select(x => x.Low), 
bars.Select(x => x.Close), bars.Select(x => x.Volume), bars.Select(x => x.TimeUtc), InputName.High);

var results = stockData.CalculateBollingerBands(MovingAvgType.EhlersMesaAdaptiveMovingAverage, 15, 2.5m);
var upperBandList = results.OutputValues["UpperBand"];
var middleBandList = results.OutputValues["MiddleBand"];
var lowerBandList = results.OutputValues["LowerBand"];
```

For more detailed Alpaca examples then check out my more advanced [Alpaca example code](https://github.com/alpacahq/alpaca-trade-api-csharp/blob/develop/UsageExamples/IndicatorLibraryExample.cs)

### Support This Project

BTC: 36DRmZefJNW82q9pHY1kWYSZhLUWQkpgGq

ETH: 0x7D6e58754476189ffF736B63b6159D2647f74f34

DOGE: DF1nsK1nLASzmwHNAfNengBGS4w7bNyJ1e

SHIB: 0xCDe2355212764218355c9393FbE121Ae49B43382

Paypal: [https://www.paypal.me/cheatcountry](https://www.paypal.me/cheatcountry)

Patreon: [https://patreon.com/cheatcountry](https://patreon.com/cheatcountry)


### Support or Contact

Email me at cheatcountry@gmail.com for any help or support.
