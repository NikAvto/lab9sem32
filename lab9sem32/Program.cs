using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        string[] tickers = File.ReadAllLines("ticker.txt");
        string fromDate = "2024-01-06"; // Начальная дата (немного не год назад, лол)
        string toDate = "2024-11-06"; // Конечная дата (текущая дата)

        foreach (var ticker in tickers)
        {
            var averagePrice = await GetDataAsync(ticker.Trim(), fromDate, toDate);
            if (averagePrice.HasValue)
            {
                Console.WriteLine($"Средняя цена для {ticker}: {averagePrice.Value}");
                WriteAverageToFile(ticker.Trim(), averagePrice.Value);
            }
        }
    }

    private static async Task<double?> GetDataAsync(string ticker, string fromDate, string toDate)
    {
        string url = $"https://api.marketdata.app/v1/stocks/candles/D/{ticker}/?from={fromDate}&to={toDate}&token=T1JFM3lweVluSEhzTWhMQk5EM2NOal9PdDUzbUFzX090cTlTSXRuT1lPdz0";
        try
        {
            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);

            // Извлечение значений high и low
            var highs = json["h"].ToObject<List<double>>();
            var lows = json["l"].ToObject<List<double>>();
            double averageHigh = CalculateAverage(highs);
            double averageLow = CalculateAverage(lows);

            // Возвращаем среднее значение из high и low
            return (averageHigh + averageLow) / 2;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении данных для {ticker}: {ex.Message}");
            return null;
        }
    }

    private static double CalculateAverage(List<double> values)
    {
        if (values.Count == 0) return 0;
        double sum = 0;
        foreach (var value in values)
        {
            sum += value;
        }
        return sum / values.Count;
    }

    private static void WriteAverageToFile(string ticker, double averagePrice)
    {
        using (StreamWriter writer = new StreamWriter("average_prices.txt", true))
        {
            writer.WriteLine($"{ticker}:{averagePrice}");
        }
    }
}