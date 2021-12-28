using Alpaca.Markets;
using OoplesFinance.StockIndicators;
using OoplesFinance.StockIndicators.Models;

namespace OoplesFinance.StockIndicators.ExampleApp
{
    public class ExampleApp
    {
        const string paperApiKey = "REPLACEME";
        const string paperApiSecret = "REPLACEME";
        const string symbol = "AAPL";

        public static async Task Run()
        {
            var secretKey = new SecretKey(paperApiKey, paperApiSecret);
            var alpacaDataClient = Environments.Paper.GetAlpacaDataClient(secretKey);

            var startDate = new DateTime(2021, 1, 1);
            var endDate = new DateTime(2021, 12, 15);
            var bars = (await alpacaDataClient.GetHistoricalBarsAsync(new HistoricalBarsRequest(symbol, startDate, endDate, BarTimeFrame.Day)).
                ConfigureAwait(false)).Items.SelectMany(x => x.Value);

            var closePrices = bars.Select(x => x.Close);
            var openPrices = bars.Select(x => x.Open);
            var highPrices = bars.Select(x => x.High);
            var lowPrices = bars.Select(x => x.Low);
            var volumes = bars.Select(x => x.Volume);

            var stockData = new StockData(openPrices, highPrices, lowPrices, closePrices, volumes);
            var results = stockData.CalculateBollingerBands();
        }
    }
}