## .Net Stock Indicator Library

This is a stock indicator library that is completely open source and very easy to use. Current version contains [103 stock indicators](https://ooples.github.io/OoplesFinance.StockIndicators/indicators) with over 700 planned for final release.

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
var secretKey = new SecretKey(paperApiKey, paperApiSecret);
var alpacaDataClient = Environments.Paper.GetAlpacaDataClient(secretKey);

var startDate = new DateTime(2021, 1, 1);
var endDate = new DateTime(2021, 12, 15);
var bars = (await alpacaDataClient.GetHistoricalBarsAsync(
	new HistoricalBarsRequest(symbol, startDate, endDate, BarTimeFrame.Day)).
ConfigureAwait(false)).Items.SelectMany(x => x.Value);

var closePrices = bars.Select(x => x.Close);
var openPrices = bars.Select(x => x.Open);
var highPrices = bars.Select(x => x.High);
var lowPrices = bars.Select(x => x.Low);
var volumes = bars.Select(x => x.Volume);

var stockData = new StockData(openPrices, highPrices, lowPrices, closePrices, volumes);
var results = stockData.CalculateBollingerBands();
```

### Support or Contact

Email me at cheatcountry@gmail.com for any help or support.
