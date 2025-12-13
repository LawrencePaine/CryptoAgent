using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using Skender.Stock.Indicators;

namespace CryptoAgent.Api.Services;

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

        var pricesResponse = await client.GetFromJsonAsync<Dictionary<string, Dictionary<string, decimal>>>("/api/v3/simple/price?ids=bitcoin,ethereum&vs_currencies=gbp");
        var btcPrice = pricesResponse != null &&
                       pricesResponse.TryGetValue("bitcoin", out var btcDict) &&
                       btcDict.TryGetValue("gbp", out var btc) ? btc : 0;
        var ethPrice = pricesResponse != null &&
                       pricesResponse.TryGetValue("ethereum", out var ethDict) &&
                       ethDict.TryGetValue("gbp", out var eth) ? eth : 0;

        try
        {
            var marketsResponse = await client.GetFromJsonAsync<List<JsonElement>>("/api/v3/coins/markets?vs_currency=gbp&ids=bitcoin,ethereum&price_change_percentage=24h,7d");

            decimal btc24h = 0, eth24h = 0, btc7d = 0, eth7d = 0;

            foreach (var item in marketsResponse ?? new List<JsonElement>())
            {
                var id = item.GetProperty("id").GetString();

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
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching market data: {ex.Message}");
            return new MarketSnapshot
            {
                TimestampUtc = DateTime.UtcNow,
                BtcPriceGbp = btcPrice,
                EthPriceGbp = ethPrice
            };
        }
    }

    public async Task<List<Quote>> GetHistoricalDataAsync(string coinId, int days)
    {
        var client = _httpClientFactory.CreateClient("coingecko");
        try
        {
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
            Console.WriteLine($"Fetched {quotes.Count} candles for {coinId}");
            return quotes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching history for {coinId}: {ex.Message}");
            return new List<Quote>();
        }
    }
}

public class RiskEngine
{
    public (LastDecision decision, Trade? trade) Apply(
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
            LlmConfidence = suggestion.Confidence,
            FinalAction = RawActionType.Hold,
            FinalAsset = AssetType.None,
            FinalSizeGbp = 0,
            Executed = false,
            RationaleShort = suggestion.RationaleShort,
            RationaleDetailed = suggestion.RationaleDetailed
        };

        if (suggestion.Action == RawActionType.Hold || suggestion.Asset == AssetType.None)
        {
            decision.RiskReason = suggestion.RationaleShort.StartsWith("LLM Error") 
                ? "LLM Failure" 
                : "LLM suggested HOLD or None";
            return (decision, null);
        }

        if (tradesToday >= config.MaxTradesPerDay)
        {
            decision.RiskReason = "Max trades per day reached";
            return (decision, null);
        }

        if (suggestion.SizeGbp <= 0)
        {
            decision.RiskReason = "Trade size <= 0";
            return (decision, null);
        }

        if (suggestion.SizeGbp > config.MaxTradeSizeGbp)
        {
            decision.RiskReason = "Trade size exceeds maximum allowed";
            return (decision, null);
        }

        decimal price = suggestion.Asset == AssetType.Btc ? market.BtcPriceGbp : market.EthPriceGbp;
        if (price <= 0)
        {
            decision.RiskReason = "Invalid price";
            return (decision, null);
        }

        if (suggestion.Action == RawActionType.Buy)
        {
            if (portfolio.CashGbp < suggestion.SizeGbp)
            {
                decision.RiskReason = "Insufficient cash";
                return (decision, null);
            }

            var change7d = suggestion.Asset == AssetType.Btc ? market.BtcChange7dPct : market.EthChange7dPct;
            if (change7d > config.MaxBuyAfter7dRallyPct)
            {
                decision.RiskReason = $"7d change {change7d:P1} exceeds limit {config.MaxBuyAfter7dRallyPct:P1}";
                return (decision, null);
            }

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
                return (decision, null);
            }
            if (newEthVal / newTotal > config.MaxEthAllocationPct)
            {
                decision.RiskReason = "Would breach Max ETH Allocation";
                return (decision, null);
            }
            if ((newCash + portfolio.VaultGbp) / newTotal < config.MinCashAllocationPct)
            {
                decision.RiskReason = "Would breach Min Cash Allocation";
                return (decision, null);
            }

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

            return (decision, trade);
        }

        if (suggestion.Action == RawActionType.Sell)
        {
            var currentAmount = suggestion.Asset == AssetType.Btc ? portfolio.BtcAmount : portfolio.EthAmount;
            var amountToSell = suggestion.SizeGbp / price;

            if (amountToSell > currentAmount)
            {
                amountToSell = currentAmount;
            }

            var adjustedSizeGbp = amountToSell * price;
            if (adjustedSizeGbp < 0.01m)
            {
                decision.RiskReason = "Sell size too small";
                return (decision, null);
            }

            decision.FinalAction = RawActionType.Sell;
            decision.FinalAsset = suggestion.Asset;
            decision.FinalSizeGbp = adjustedSizeGbp;
            decision.Executed = true;
            decision.RiskReason = "Accepted";

            var trade = new Trade
            {
                TimestampUtc = DateTime.UtcNow,
                Asset = suggestion.Asset,
                Action = RawActionType.Sell,
                SizeGbp = adjustedSizeGbp,
                PriceGbp = price
            };

            return (decision, trade);
        }

        return (decision, null);
    }
}

public class AgentService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MarketDataService _marketDataService;
    private readonly RiskEngine _riskEngine;
    private readonly ChatClient _chatClient;
    private readonly RiskConfig _riskConfig;
    private readonly AppConfig _appConfig;

    public LastDecision? LastDecision { get; private set; }

    public AgentService(
        IServiceScopeFactory scopeFactory,
        MarketDataService marketDataService,
        RiskEngine riskEngine,
        ChatClient chatClient,
        RiskConfig riskConfig,
        AppConfig appConfig)
    {
        _scopeFactory = scopeFactory;
        _marketDataService = marketDataService;
        _riskEngine = riskEngine;
        _chatClient = chatClient;
        _riskConfig = riskConfig;
        _appConfig = appConfig;
    }

    public async Task RunOnceAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var portfolioRepository = scope.ServiceProvider.GetRequiredService<PortfolioRepository>();
        var performanceRepository = scope.ServiceProvider.GetRequiredService<PerformanceRepository>();
        var decisionRepository = scope.ServiceProvider.GetRequiredService<DecisionRepository>();
        var exchangeClient = scope.ServiceProvider.GetRequiredService<IExchangeClient>();
        var llmStateBuilder = scope.ServiceProvider.GetRequiredService<LlmStateBuilder>();

        var portfolio = await portfolioRepository.GetAsync();
        var market = await _marketDataService.GetSnapshotAsync();

        var btcHistory = await _marketDataService.GetHistoricalDataAsync("bitcoin", 365);
        var ethHistory = await _marketDataService.GetHistoricalDataAsync("ethereum", 365);

        market.BtcTechnical = CalculateTechnical(btcHistory);
        market.EthTechnical = CalculateTechnical(ethHistory);

        var llmState = await llmStateBuilder.BuildAsync(portfolio, market);
        var tradesToday = llmState.Risk.TradesToday;

        var stateJson = JsonSerializer.Serialize(llmState, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var systemPrompt = @"You are a cautious crypto trading agent managing a paper portfolio.

You trade ONLY BTC and ETH, sized in GBP. No leverage. No derivatives. No shorting.
You MUST consider existing positions, allocations, cash remaining, fees, risk constraints, and recent trades/decisions before recommending any action.

You will be given a JSON state object. Use ONLY values present in that state.
Do NOT invent indicator values or portfolio amounts.
If indicators are missing or null, do NOT use them; prefer HOLD.

Output MUST be strict JSON only (no markdown, no extra text) matching exactly this schema:
{
  ""Action"": ""Buy"" | ""Sell"" | ""Hold"",
  ""Asset"": ""Btc"" | ""Eth"" | ""None"",
  ""SizeGbp"": number,
  ""Confidence"": number,
  ""RationaleShort"": ""string"",
  ""RationaleDetailed"": ""string"",
  ""Notes"": ""string""
}

Rules:
1) Prefer HOLD unless there is a clear, small, risk-compliant opportunity.
2) Never recommend a trade exceeding MaxTradeSizeGbp or violating allocations/min cash.
3) Avoid repeated buys in the same direction unless explicitly justified and within constraints.
4) If TradesToday >= MaxTradesPerDay, output HOLD.
5) RationaleShort must be one sentence.
6) RationaleDetailed must be 3–6 short sentences referencing only provided values (portfolio, prices, changes, recent trades/decisions, indicators if present).";

        var userPrompt = $@"Use ONLY the JSON state below to decide Buy/Sell/Hold.
Return exactly one JSON object matching the required schema, nothing else.

STATE_JSON:
{stateJson}";

        LlmSuggestion suggestion;
        string modelOutput = string.Empty;
        try
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                });

            modelOutput = completion.Content[0].Text;
            modelOutput = modelOutput.Replace("```json", "").Replace("```", "").Trim();

            suggestion = JsonSerializer.Deserialize<LlmSuggestion>(modelOutput, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? BuildInvalidSuggestion();
        }
        catch (JsonException ex)
        {
            suggestion = BuildInvalidSuggestion(ex.Message);
        }
        catch (Exception ex)
        {
            suggestion = new LlmSuggestion
            {
                Action = RawActionType.Hold,
                Asset = AssetType.None,
                SizeGbp = 0,
                Confidence = 0,
                RationaleShort = $"LLM Error: {ex.Message}",
                RationaleDetailed = string.Empty,
                Notes = $"Exception: {ex.Message}"
            };
            Console.WriteLine($"LLM Exception: {ex}");
        }

        suggestion ??= BuildInvalidSuggestion();

        var (decision, trade) = _riskEngine.Apply(suggestion, portfolio, market, tradesToday, _riskConfig);
        decision.Mode = _appConfig.Mode.ToString().ToUpperInvariant();
        decision.ProviderUsed = "OpenAI";
        decision.LlmConfidence = suggestion.Confidence;
        decision.RawModelOutput = modelOutput;
        LastDecision = decision;

        await decisionRepository.AddAsync(decision);

        if (decision.Executed && trade != null)
        {
            var side = trade.Action == RawActionType.Buy ? OrderSide.Buy : OrderSide.Sell;
            var symbol = trade.Asset == AssetType.Btc ? "BTC" : "ETH";

            var executedTrade = await exchangeClient.PlaceMarketOrderAsync(symbol, trade.SizeGbp, side);
            decision.FinalSizeGbp = executedTrade.SizeGbp;
            decision.RiskReason = "Executed";
        }

        portfolio = await portfolioRepository.GetAsync();
        var dto = portfolio.ToDto(market);

        if (portfolio.HighWatermarkGbp == 0)
        {
            portfolio.HighWatermarkGbp = dto.TotalValueGbp;
            await portfolioRepository.SaveAsync(portfolio);
        }
        else if (dto.TotalValueGbp > portfolio.HighWatermarkGbp * 1.10m)
        {
            var profit = dto.TotalValueGbp - portfolio.HighWatermarkGbp;
            var skimAmount = profit * 0.30m;

            if (portfolio.CashGbp >= skimAmount)
            {
                portfolio.CashGbp -= skimAmount;
                portfolio.VaultGbp += skimAmount;
                portfolio.HighWatermarkGbp = dto.TotalValueGbp;
                await portfolioRepository.SaveAsync(portfolio);
            }
        }

        var allPerf = await performanceRepository.GetAllAsync();
        var lastCumulatedAi = allPerf.LastOrDefault()?.CumulatedAiCostGbp ?? 0;
        var totalFees = await portfolioRepository.GetTotalFeesAsync();

        var perf = new PerformanceSnapshot
        {
            DateUtc = DateTime.UtcNow,
            PortfolioValueGbp = dto.TotalValueGbp,
            VaultGbp = portfolio.VaultGbp,
            NetDepositsGbp = 0,
            CumulatedAiCostGbp = lastCumulatedAi + _appConfig.EstimatedAiCostPerRunGbp,
            CumulatedFeesGbp = totalFees
        };
        await performanceRepository.AppendAsync(perf);

        LlmSuggestion BuildInvalidSuggestion(string? error = null)
        {
            var reason = "Model output invalid; defaulting to HOLD.";
            var notes = reason;
            if (!string.IsNullOrWhiteSpace(error))
            {
                notes += $" Error: {error}";
            }

            if (!string.IsNullOrWhiteSpace(modelOutput))
            {
                notes += $" Raw: {modelOutput}";
            }

            return new LlmSuggestion
            {
                Action = RawActionType.Hold,
                Asset = AssetType.None,
                SizeGbp = 0,
                Confidence = 0,
                RationaleShort = reason,
                RationaleDetailed = string.Empty,
                Notes = notes
            };
        }
    }

    private TechnicalAnalysis? CalculateTechnical(List<Quote> history)
    {
        if (history.Count < 50) return null;

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
