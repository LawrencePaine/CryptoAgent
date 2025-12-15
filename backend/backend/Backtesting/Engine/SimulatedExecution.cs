using CryptoAgent.Api.Backtesting.Entities;

namespace CryptoAgent.Api.Backtesting.Engine;

public class SimulatedExecution
{
    public (decimal CashGbp, decimal AssetAmount, BacktestTradeEntity Trade) Buy(
        decimal cashGbp,
        decimal price,
        decimal sizeGbp,
        decimal feePct,
        decimal slippagePct,
        string asset)
    {
        var effectivePrice = price * (1 + slippagePct);
        var fee = sizeGbp * feePct;
        var cost = sizeGbp + fee;
        var units = effectivePrice == 0 ? 0 : sizeGbp / effectivePrice;
        var trade = new BacktestTradeEntity
        {
            Asset = asset,
            Side = "BUY",
            SizeGbp = sizeGbp,
            PriceGbp = effectivePrice,
            FeeGbp = fee,
            SlippageGbp = sizeGbp * slippagePct
        };

        return (cashGbp - cost, units, trade);
    }

    public (decimal CashGbp, decimal AssetAmount, BacktestTradeEntity Trade) Sell(
        decimal cashGbp,
        decimal price,
        decimal assetAmount,
        decimal sizeGbp,
        decimal feePct,
        decimal slippagePct,
        string asset)
    {
        var effectivePrice = price * (1 - slippagePct);
        var desiredUnits = effectivePrice == 0 ? 0 : sizeGbp / effectivePrice;
        var units = Math.Min(assetAmount, desiredUnits);
        var proceeds = units * effectivePrice;
        var fee = proceeds * feePct;
        var trade = new BacktestTradeEntity
        {
            Asset = asset,
            Side = "SELL",
            SizeGbp = proceeds,
            PriceGbp = effectivePrice,
            FeeGbp = fee,
            SlippageGbp = units * price * slippagePct
        };

        return (cashGbp + proceeds - fee, assetAmount - units, trade);
    }
}
