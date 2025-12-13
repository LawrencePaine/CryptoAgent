using CryptoAgent.Api.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CryptoAgent.Api.Services;

public class AgentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AgentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var marketDataService = scope.ServiceProvider.GetRequiredService<MarketDataService>();
            var agentService = scope.ServiceProvider.GetRequiredService<AgentService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<CryptoAgentDbContext>();

            var snapshot = await marketDataService.GetSnapshotAsync();
            await SaveMarketSnapshotAsync(dbContext, snapshot, stoppingToken);

            await agentService.RunOnceAsync();

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
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
}
