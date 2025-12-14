using CryptoAgent.Api.Data;
using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CryptoAgent.Api.Services;

public class AgentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly WorkerConfig _workerConfig;

    public AgentWorker(IServiceScopeFactory scopeFactory, WorkerConfig workerConfig)
    {
        _scopeFactory = scopeFactory;
        _workerConfig = workerConfig;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastSnapshotMinute = -1;
        DateTime? lastCandleHour = null;
        DateTime? lastDecisionHour = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var marketDataService = scope.ServiceProvider.GetRequiredService<MarketDataService>();
            var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CryptoAgentDbContext>();
            var riskConfig = scope.ServiceProvider.GetRequiredService<RiskConfig>();
            var candleRepo = scope.ServiceProvider.GetRequiredService<HourlyCandleRepository>();
            var featureCalculator = scope.ServiceProvider.GetRequiredService<HourlyFeatureCalculator>();
            var featureRepo = scope.ServiceProvider.GetRequiredService<HourlyFeatureRepository>();
            var regimeClassifier = scope.ServiceProvider.GetRequiredService<RegimeClassifier>();
            var regimeRepo = scope.ServiceProvider.GetRequiredService<RegimeStateRepository>();
            var strategyRepo = scope.ServiceProvider.GetRequiredService<StrategySignalRepository>();
            var portfolioRepo = scope.ServiceProvider.GetRequiredService<PortfolioRepository>();
            var strategies = scope.ServiceProvider.GetServices<IStrategyModule>().ToList();

            var now = DateTime.UtcNow;
            var currentHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

            if (now.Minute % _workerConfig.SnapshotMinutes == 0 && lastSnapshotMinute != now.Minute)
            {
                var snapshot = await marketDataService.GetSnapshotAsync();
                await SaveMarketSnapshotAsync(dbContext, snapshot, stoppingToken);
                lastSnapshotMinute = now.Minute;
            }

            if (now.Minute == _workerConfig.HourlyCandleFetchMinute && lastCandleHour != currentHour)
            {
                await FetchHourlyCandlesAsync(marketDataService, candleRepo, currentHour, stoppingToken);
                lastCandleHour = currentHour;
            }

            if (now.Minute == _workerConfig.RunDecisionMinute && lastDecisionHour != currentHour)
            {
                await RunHourlyPipelineAsync(marketDataService, portfolioRepo, candleRepo, featureCalculator, featureRepo, regimeClassifier, regimeRepo, strategies, strategyRepo, riskConfig, stoppingToken);
                await agentService.RunOnceAsync();
                lastDecisionHour = currentHour;
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private static async Task SaveMarketSnapshotAsync(CryptoAgentDbContext dbContext, Models.MarketSnapshot snapshot, CancellationToken cancellationToken)
    {
        var entity = new MarketSnapshotEntity
        {
            TimestampUtc = snapshot.TimestampUtc,
            BtcPriceGbp = snapshot.BtcPriceGbp,
            EthPriceGbp = snapshot.EthPriceGbp,
            BtcChange24hPct = snapshot.BtcChange24hPct,
            EthChange24hPct = snapshot.EthChange24hPct,
            BtcChange7dPct = snapshot.BtcChange7dPct,
            EthChange7dPct = snapshot.EthChange7dPct
        };

        dbContext.MarketSnapshots.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task FetchHourlyCandlesAsync(MarketDataService marketDataService, HourlyCandleRepository candleRepo, DateTime currentHour, CancellationToken ct)
    {
        foreach (var asset in new[] { "BTC", "ETH" })
        {
            var latest = await candleRepo.GetLatestAsync(asset);
            var from = latest?.HourUtc.AddHours(1) ?? currentHour.AddHours(-200);
            if (from > currentHour) continue;

            var candles = await marketDataService.GetHourlyCandlesAsync(asset, from, currentHour, ct);
            var entities = candles.Select(c => new Data.Entities.HourlyCandleEntity
            {
                Asset = c.Asset,
                HourUtc = c.HourUtc,
                OpenGbp = c.OpenGbp,
                HighGbp = c.HighGbp,
                LowGbp = c.LowGbp,
                CloseGbp = c.CloseGbp,
                Volume = c.Volume,
                Source = c.Source
            });

            await candleRepo.UpsertBatchAsync(entities);
        }
    }

    private static async Task RunHourlyPipelineAsync(
        MarketDataService marketDataService,
        PortfolioRepository portfolioRepository,
        HourlyCandleRepository candleRepo,
        HourlyFeatureCalculator featureCalculator,
        HourlyFeatureRepository featureRepo,
        RegimeClassifier classifier,
        RegimeStateRepository regimeRepo,
        List<IStrategyModule> strategies,
        StrategySignalRepository signalRepository,
        RiskConfig riskConfig,
        CancellationToken ct)
    {
        var market = await marketDataService.GetSnapshotAsync();
        var portfolio = await portfolioRepository.GetAsync();
        portfolio.BtcValueGbp = portfolio.BtcAmount * market.BtcPriceGbp;
        portfolio.EthValueGbp = portfolio.EthAmount * market.EthPriceGbp;
        portfolio.TotalValueGbp = portfolio.CashGbp + portfolio.VaultGbp + portfolio.BtcValueGbp + portfolio.EthValueGbp;

        foreach (var asset in new[] { "BTC", "ETH" })
        {
            var to = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc);
            var from = to.AddHours(-168);
            var window = await candleRepo.GetWindowAsync(asset, from, to);
            if (window.Count < 2) continue;

            var feature = featureCalculator.CalculateLatest(asset, window);
            if (feature == null) continue;

            await featureRepo.UpsertAsync(feature);
            var regime = classifier.Classify(feature);
            await regimeRepo.UpsertAsync(regime);

            var signals = new List<Data.Entities.StrategySignalEntity>();
            foreach (var strategy in strategies)
            {
                var signal = await strategy.EvaluateAsync(asset, feature, regime, portfolio, riskConfig, ct);
                signals.Add(new Data.Entities.StrategySignalEntity
                {
                    Asset = signal.Asset,
                    HourUtc = signal.HourUtc,
                    StrategyName = signal.StrategyName,
                    SignalScore = signal.SignalScore,
                    SuggestedAction = signal.SuggestedAction,
                    SuggestedSizeGbp = signal.SuggestedSizeGbp,
                    Reason = signal.Reason
                });
            }

            await signalRepository.UpsertBatchAsync(signals);
        }
    }
}
