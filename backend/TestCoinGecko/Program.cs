using System.Net.Http.Json;
using System.Text.Json;

var client = new HttpClient();
client.BaseAddress = new Uri("https://api.coingecko.com");
client.DefaultRequestHeaders.Add("User-Agent", "CryptoAgentPOC/1.0");
client.DefaultRequestHeaders.Add("x-cg-demo-api-key", "CG-dSrEocPiKzFiBBaee5mdxQtH");

var days = 100;
var coinId = "bitcoin";

try
{
    Console.WriteLine($"Fetching {coinId} for {days} days...");
    // Raw string first
    var response = await client.GetAsync($"/api/v3/coins/{coinId}/ohlc?vs_currency=gbp&days={days}");
    Console.WriteLine($"Status: {response.StatusCode}");
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"Content Sample: {content.Substring(0, Math.Min(500, content.Length))}");

    var data = JsonSerializer.Deserialize<List<List<decimal>>>(content);
    Console.WriteLine($"Count: {data?.Count}");
    if (data != null && data.Count > 0)
    {
         // Verify first candle timestamp
         var first = data[0];
         Console.WriteLine($"First Candle: {first[0]}");
         var date = DateTimeOffset.FromUnixTimeMilliseconds((long)first[0]).UtcDateTime;
         Console.WriteLine($"First Date: {date}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex}");
}
