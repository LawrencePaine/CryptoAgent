using System.Net.Http;
using System.Text.Json;
using CryptoAgent.Api.Models;
using OpenAI.Chat;
using Skender.Stock.Indicators;

namespace CryptoAgent.Api.Services;

public class PortfolioStore
{
    private readonly string _filePath;
    private readonly string _tradesPath;

    public PortfolioStore()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "portfolio.json");
        _tradesPath = Path.Combine(AppContext.BaseDirectory, "trades.json");
    }

    public async Task<Portfolio> GetAsync()
    {
        if (!File.Exists(_filePath))
        {
            var initial = new Portfolio { CashGbp = 50m };
            await SaveAsync(initial);
            return initial;
        }

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<Portfolio>(json) ?? new Portfolio { CashGbp = 50m };
    }

    public async Task SaveAsync(Portfolio portfolio)
    {
        var json = JsonSerializer.Serialize(portfolio, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }

    public async Task LogTradeAsync(Trade trade)
    {
        var trades = await GetAllTradesAsync();
        trades.Add(trade);
        var json = JsonSerializer.Serialize(trades, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_tradesPath, json);
    }

    public async Task<List<Trade>> GetRecentTradesAsync(int count)
    {
        var trades = await GetAllTradesAsync();
        return trades.OrderByDescending(t => t.TimestampUtc).Take(count).ToList();
    }

    public async Task<int> CountTradesTodayAsync()
    {
        var trades = await GetAllTradesAsync();
        var today = DateTime.UtcNow.Date;
        return trades.Count(t => t.TimestampUtc.Date == today);
    }

    private async Task<List<Trade>> GetAllTradesAsync()
    {
        if (!File.Exists(_tradesPath)) return new List<Trade>();
        var json = await File.ReadAllTextAsync(_tradesPath);
        return JsonSerializer.Deserialize<List<Trade>>(json) ?? new List<Trade>();
    }
}

public class MarketDataService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MarketDataService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<MarketSnapshot> GetSnapshotAsync()
    {
        var client = _httpClientFactory.CreateClient("coingecko");
        
        // Get prices
        var pricesResponse = await client.GetFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>("/api/v3/simple/price?ids=bitcoin,ethereum&vs_currencies=gbp");
        var btcPrice = pricesResponse?["bitcoin"]["gbp"] ?? 0;
        var ethPrice = pricesResponse?["ethereum"]["gbp"] ?? 0;

        // Get changes
        // Note: CoinGecko API structure for /coins/markets is a list of objects
        // We need to handle potential API errors or empty responses gracefully
        try 
        {
            var marketsResponse = await client.GetFromJsonAsync<List<JsonElement>>("/api/v3/coins/markets?vs_currency=gbp&ids=bitcoin,ethereum&price_change_percentage=24h,7d");
            
            decimal btc24h = 0, eth24h = 0, btc7d = 0, eth7d = 0;

            foreach (var item in marketsResponse ?? new List<JsonElement>())
            {
                var id = item.GetProperty("id").GetString();
                
                // Safe property access
                decimal change24h = 0;
                if (item.TryGetProperty("price_change_percentage_24h", out var p24h) && p24h.ValueKind == JsonValueKind.Number)
                    change24h = p24h.GetDecimal();

                decimal change7d = 0;
                if (item.TryGetProperty("price_change_percentage_7d_in_currency", out var p7d) && p7d.ValueKind == JsonValueKind.Number)
                    change7d = p7d.GetDecimal();

                if (id == "bitcoin")
                {
                    btc24h = change24h;
                    btc7d = change7d;
                }
                else if (id == "ethereum")
                {
                    eth24h = change24h;
                    eth7d = change7d;
                }
            }

            return new MarketSnapshot
            {
                TimestampUtc = DateTime.UtcNow,
                BtcPriceGbp = btcPrice,
                EthPriceGbp = ethPrice,
                BtcChange24hPct = btc24h / 100m,
                EthChange24hPct = eth24h / 100m,
                BtcChange7dPct = btc7d / 100m,
                EthChange7dPct = eth7d / 100m
            };
        }
        catch
        {
            // Fallback if markets endpoint fails but prices succeeded
             return new MarketSnapshot
            {
                TimestampUtc = DateTime.UtcNow,
                BtcPriceGbp = btcPrice,
                EthPriceGbp = ethPrice
            };
            };
        }
    };

    public async Task<List<Quote>> GetHistoricalDataAsync(string coinId, int days)
    {
        var client = HttpClientFactory.CreateClient("coingecko");
        try
        {
            // CoinGecko OHLC: [[time, open, high, low, close, volume], ...]
            var data = await client.GetFromJsonAsync<List<List<decimal>>>($"/api/v3/coins/{coinId}/ohlc?vs_currency=gbp&days={days}");
            
            var quotes = new List<Quote>();
            if (data == null) return quotes;

            foreach (var candle in data)
            {
                if (candle.Count < 5) continue;

                quotes.Add(new Quote
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds((long)candle[0]).UtcDateTime,
                    Open = candle[1],
                    High = candle[2],
                    Low = candle[3],
                    Close = candle[4],
                    Volume = candle.Count > 5 ? candle[5] : 0
                });
            }
            return quotes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching history for {coinId}: {ex.Message}");
            return new List<Quote>();
        }
    }
}

public class PerformanceStore
{
    private readonly string _filePath;

    public PerformanceStore()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "performance.json");
    }

    public async Task<List<PerformanceSnapshot>> GetAllAsync()
    {
        if (!File.Exists(_filePath)) return new List<PerformanceSnapshot>();
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<PerformanceSnapshot>>(json) ?? new List<PerformanceSnapshot>();
    }

    public async Task AppendAsync(PerformanceSnapshot snapshot)
    {
        var list = await GetAllAsync();
        list.Add(snapshot);
        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }
}

public class RiskEngine
{
    public (LastDecision decision, Portfolio updatedPortfolio, Trade? trade) Apply(
        LlmSuggestion suggestion, 
        Portfolio portfolio, 
        MarketSnapshot market, 
        int tradesToday, 
        RiskConfig config)
    {
        var decision = new LastDecision
        {
            TimestampUtc = DateTime.UtcNow,
            LlmAction = suggestion.Action,
            LlmAsset = suggestion.Asset,
            LlmSizeGbp = suggestion.SizeGbp,
            FinalAction = RawActionType.Hold,
            FinalAsset = AssetType.None,
            FinalSizeGbp = 0,
            Executed = false,
            RationaleShort = suggestion.RationaleShort
        };

        // 1. Basic checks
        if (suggestion.Action == RawActionType.Hold || suggestion.Asset == AssetType.None)
        {
            decision.RiskReason = "LLM suggested HOLD or None";
            return (decision, portfolio, null);
        }

        if (tradesToday >= config.MaxTradesPerDay)
        {
            decision.RiskReason = "Max trades per day reached";
            return (decision, portfolio, null);
        }

        if (suggestion.SizeGbp <= 0)
        {
            decision.RiskReason = "Trade size <= 0";
            return (decision, portfolio, null);
        }

        decimal price = suggestion.Asset == AssetType.Btc ? market.BtcPriceGbp : market.EthPriceGbp;
        if (price <= 0)
        {
            decision.RiskReason = "Invalid price";
            return (decision, portfolio, null);
        }

        // 2. Buy checks
        if (suggestion.Action == RawActionType.Buy)
        {
            if (portfolio.CashGbp < suggestion.SizeGbp)
            {
                decision.RiskReason = "Insufficient cash";
                return (decision, portfolio, null);
            }

            // 7d rally check
            var change7d = suggestion.Asset == AssetType.Btc ? market.BtcChange7dPct : market.EthChange7dPct;
            if (change7d > config.MaxBuyAfter7dRallyPct)
            {
                decision.RiskReason = $"7d change {change7d:P1} exceeds limit {config.MaxBuyAfter7dRallyPct:P1}";
                return (decision, portfolio, null);
            }

            // Allocation check (simulate)
            var amountToBuy = suggestion.SizeGbp / price;
            var newBtc = portfolio.BtcAmount + (suggestion.Asset == AssetType.Btc ? amountToBuy : 0);
            var newEth = portfolio.EthAmount + (suggestion.Asset == AssetType.Eth ? amountToBuy : 0);
            var newCash = portfolio.CashGbp - suggestion.SizeGbp;
            
            var newBtcVal = newBtc * market.BtcPriceGbp;
            var newEthVal = newEth * market.EthPriceGbp;
            var newTotal = newCash + portfolio.VaultGbp + newBtcVal + newEthVal;

            if (newBtcVal / newTotal > config.MaxBtcAllocationPct)
            {
                decision.RiskReason = "Would breach Max BTC Allocation";
                return (decision, portfolio, null);
            }
            if (newEthVal / newTotal > config.MaxEthAllocationPct)
            {
                decision.RiskReason = "Would breach Max ETH Allocation";
                return (decision, portfolio, null);
            }
            if ((newCash + portfolio.VaultGbp) / newTotal < config.MinCashAllocationPct)
            {
                decision.RiskReason = "Would breach Min Cash Allocation";
                return (decision, portfolio, null);
            }

            // Accepted
            portfolio.CashGbp = newCash;
            if (suggestion.Asset == AssetType.Btc) portfolio.BtcAmount = newBtc;
            else portfolio.EthAmount = newEth;

            decision.FinalAction = RawActionType.Buy;
            decision.FinalAsset = suggestion.Asset;
            decision.FinalSizeGbp = suggestion.SizeGbp;
            decision.Executed = true;
            decision.RiskReason = "Accepted";

            var trade = new Trade
            {
                TimestampUtc = DateTime.UtcNow,
                Asset = suggestion.Asset,
                Action = RawActionType.Buy,
                SizeGbp = suggestion.SizeGbp,
                PriceGbp = price
            };

            return (decision, portfolio, trade);
        }

        // 3. Sell checks
        if (suggestion.Action == RawActionType.Sell)
        {
            var currentAmount = suggestion.Asset == AssetType.Btc ? portfolio.BtcAmount : portfolio.EthAmount;
            var amountToSell = suggestion.SizeGbp / price;

            if (amountToSell > currentAmount)
            {
                 // Cap at max available
                 amountToSell = currentAmount;
                 suggestion.SizeGbp = amountToSell * price; // Adjust size
            }
            
            if (suggestion.SizeGbp < 0.01m) // Min trade size check effectively
            {
                 decision.RiskReason = "Sell size too small";
                 return (decision, portfolio, null);
            }

            // Accepted
            portfolio.CashGbp += suggestion.SizeGbp;
            if (suggestion.Asset == AssetType.Btc) portfolio.BtcAmount -= amountToSell;
            else portfolio.EthAmount -= amountToSell;

            decision.FinalAction = RawActionType.Sell;
            decision.FinalAsset = suggestion.Asset;
            decision.FinalSizeGbp = suggestion.SizeGbp;
            decision.Executed = true;
            decision.RiskReason = "Accepted";

             var trade = new Trade
            {
                TimestampUtc = DateTime.UtcNow,
                Asset = suggestion.Asset,
                Action = RawActionType.Sell,
                SizeGbp = suggestion.SizeGbp,
                PriceGbp = price
            };

            return (decision, portfolio, trade);
        }

        return (decision, portfolio, null);
    }
}

public class AgentService
{
    private readonly PortfolioStore _portfolioStore;
    private readonly MarketDataService _marketDataService;
    private readonly PerformanceStore _performanceStore;
    private readonly RiskEngine _riskEngine;
    private readonly ChatClient _chatClient;
    private readonly RiskConfig _riskConfig;
    private readonly AppConfig _appConfig;

    public LastDecision? LastDecision { get; private set; }

    public AgentService(
        PortfolioStore portfolioStore,
        MarketDataService marketDataService,
        PerformanceStore performanceStore,
        RiskEngine riskEngine,
        ChatClient chatClient,
        RiskConfig riskConfig,
        AppConfig appConfig)
    {
        _portfolioStore = portfolioStore;
        _marketDataService = marketDataService;
        _performanceStore = performanceStore;
        _riskEngine = riskEngine;
        _chatClient = chatClient;
        _riskConfig = riskConfig;
        _appConfig = appConfig;
    }

    public async Task RunOnceAsync()
    {
        var portfolio = await _portfolioStore.GetAsync();
        var market = await _marketDataService.GetSnapshotAsync();
        
        // Fetch history and calc indicators
        var btcHistory = await _marketDataService.GetHistoricalDataAsync("bitcoin", 60);
        var ethHistory = await _marketDataService.GetHistoricalDataAsync("ethereum", 60);

        market.BtcTechnical = CalculateTechnical(btcHistory);
        market.EthTechnical = CalculateTechnical(ethHistory);

        var tradesToday = await _portfolioStore.CountTradesTodayAsync();

        var dto = portfolio.ToDto(market);

        var state = new
        {
            Portfolio = dto,
            Market = market,
            TradesToday = tradesToday,
            RiskConfig = _riskConfig
        };

        var systemPrompt = @"You are a cautious crypto trading agent managing a paper portfolio.
You only trade BTC and ETH in GBP.
You MUST output strict JSON matching this schema:
{
  ""Action"": ""Buy"" | ""Sell"" | ""Hold"",
  ""Asset"": ""Btc"" | ""Eth"" | ""None"",
  ""SizeGbp"": number,
  ""Confidence"": number (0-1),
  ""RationaleShort"": ""string"",
  ""RationaleDetailed"": ""string"",
  ""Notes"": ""string""
}
Rules:
1. No leverage.
2. Only Buy/Sell/Hold.
3. Be conservative. Prefer Hold unless there is a clear opportunity.
4. Do not hallucinate funds.
5. Use Technical Analysis (RSI, SMA, MACD) to guide your decision.
   - RSI > 70 is Overbought (Sell signal).
   - RSI < 30 is Oversold (Buy signal).
   - Price > SMA50 is Bullish Trend.
   - MACD Histogram > 0 is Bullish Momentum.
";

        var userPrompt = JsonSerializer.Serialize(state);

        LlmSuggestion suggestion;
        try 
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                });
            
            var text = completion.Content[0].Text;
            // Simple cleanup if markdown code blocks are used
            text = text.Replace("```json", "").Replace("```", "").Trim();
            
            suggestion = JsonSerializer.Deserialize<LlmSuggestion>(text) ?? new LlmSuggestion { Action = RawActionType.Hold };
        }
        catch
        {
            suggestion = new LlmSuggestion { Action = RawActionType.Hold, RationaleShort = "LLM Error" };
        }

        var (decision, updatedPortfolio, trade) = _riskEngine.Apply(suggestion, portfolio, market, tradesToday, _riskConfig);
        
        LastDecision = decision;

        if (decision.Executed && trade != null)
        {
            await _portfolioStore.SaveAsync(updatedPortfolio);
            await _portfolioStore.LogTradeAsync(trade);
            portfolio = updatedPortfolio; // Update local ref
        }

        // Profit skimming
        // Re-calc equity
        dto = portfolio.ToDto(market);
        
        if (portfolio.HighWatermarkGbp == 0)
        {
            portfolio.HighWatermarkGbp = dto.TotalValueGbp;
            await _portfolioStore.SaveAsync(portfolio);
        }
        else if (dto.TotalValueGbp > portfolio.HighWatermarkGbp * 1.10m) // 10% threshold
        {
            var profit = dto.TotalValueGbp - portfolio.HighWatermarkGbp;
            var skimAmount = profit * 0.30m; // 30% skim
            
            if (portfolio.CashGbp >= skimAmount) // Only skim from cash
            {
                portfolio.CashGbp -= skimAmount;
                portfolio.VaultGbp += skimAmount;
                portfolio.HighWatermarkGbp = dto.TotalValueGbp;
                await _portfolioStore.SaveAsync(portfolio);
            }
        }

        // Performance snapshot
        var allPerf = await _performanceStore.GetAllAsync();
        var lastCumulated = allPerf.LastOrDefault()?.CumulatedAiCostGbp ?? 0;

        var perf = new PerformanceSnapshot
        {
            DateUtc = DateTime.UtcNow,
            PortfolioValueGbp = dto.TotalValueGbp,
            VaultGbp = portfolio.VaultGbp,
            NetDepositsGbp = 0,
            CumulatedAiCostGbp = lastCumulated + _appConfig.EstimatedAiCostPerRunGbp
        };
        await _performanceStore.AppendAsync(perf);
    }

    private TechnicalAnalysis? CalculateTechnical(List<Quote> history)
    {
        if (history.Count < 50) return null; // Need enough data for SMA50

        var rsi = history.GetRsi(14).LastOrDefault()?.Rsi ?? 0;
        var sma = history.GetSma(50).LastOrDefault()?.Sma ?? 0;
        var macd = history.GetMacd(12, 26, 9).LastOrDefault();

        return new TechnicalAnalysis
        {
            Rsi14 = (decimal)rsi,
            Sma50 = (decimal)sma,
            MacdValue = (decimal)(macd?.Macd ?? 0),
            MacdSignal = (decimal)(macd?.Signal ?? 0),
            MacdHistogram = (decimal)(macd?.Histogram ?? 0)
        };
    }
}
