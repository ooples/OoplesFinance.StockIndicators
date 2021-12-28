## .Net Stock Indicator Library

This is a stock indicator library that is completely open source and I have written code for over 700 indicators. For my alpha version I published around 15 indicators with many more to be released in the next few weeks. 

### How to use this library

Calculate an indicator of your choice or go crazy and create an indicator of an indicator of an indicator. I have created this library will full customization in mind so feel free to change any settings, change the moving average used for different indicators, or come up with a crazy combo like getting the RSI of a MACD and wrap that up in a Bollinger Band of the result. Here is an easy example to get you started using the [Alpaca C# Api](https://github.com/alpacahq/alpaca-trade-api-csharp)

```cs
using Alpaca.Markets;
using OoplesFinance.StockIndicators.Models;
using static OoplesFinance.StockIndicators.Calculations;

const string paperApiKey = "REPLACEME";
const string paperApiSecret = "REPLACEME";
const string symbol = "AAPL";
var secretKey = new SecretKey(paperApiKey, paperApiSecret);
var alpacaDataClient = Environments.Paper.GetAlpacaDataClient(secretKey);
var bars = (await alpacaDataClient.GetHistoricalBarsAsync(new HistoricalBarsRequest(symbol, new DateTime(2021, 1, 1), 
    new DateTime(2021, 12, 15), BarTimeFrame.Day)).ConfigureAwait(false)).Items.SelectMany(x => x.Value);

var closePrices = bars.Select(x => x.Close);
var openPrices = bars.Select(x => x.Open);
var highPrices = bars.Select(x => x.High);
var lowPrices = bars.Select(x => x.Low);
var volumes = bars.Select(x => x.Volume);

var stockData = new StockData(openPrices, highPrices, lowPrices, closePrices, volumes);
var results = stockData.CalculateRelativeStrengthIndex().CalculateMovingAverageConvergenceDivergence().CalculateBollingerBands();
```




### Support or Contact

Email me at cheatcountry@gmail.com for any help or support.
