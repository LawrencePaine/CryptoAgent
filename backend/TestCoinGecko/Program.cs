using System.Net.Http.Json;
using System.Text.Json;

var client = new HttpClient();
client.BaseAddress = new Uri("https://api.coingecko.com");
client.DefaultRequestHeaders.Add("User-Agent", "CryptoAgentPOC/1.0");
client.DefaultRequestHeaders.Add("x-cg-demo-api-key", "CG-dSrEocPiKzFiBBaee5mdxQtH");

var days = 365;
var coinId = "bitcoin";

try
{
    Console.WriteLine($"Fetching {coinId} for {days} days...");
    // Raw string first
    var response = await client.GetAsync($"/api/v3/coins/{coinId}/ohlc?vs_currency=gbp&days={days}");
    Console.WriteLine($"Status: {response.StatusCode}");
    var content = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"CONTENT_START");
    Console.WriteLine(content);
    Console.WriteLine($"CONTENT_END");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex}");
}
