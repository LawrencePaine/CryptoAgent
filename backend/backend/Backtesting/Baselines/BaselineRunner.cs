using CryptoAgent.Api.Backtesting.Entities;

namespace CryptoAgent.Api.Backtesting.Baselines;

public class BaselineRunner
{
    public (decimal Hodl, decimal Dca, decimal Rebalance) RunBaselines(IEnumerable<(DateTime HourUtc, decimal BtcPrice, decimal EthPrice)> candles, decimal initialCash, decimal feePct, decimal slippagePct)
    {
        // Minimal placeholder calculations for now.
        var start = candles.OrderBy(c => c.HourUtc).FirstOrDefault();
        if (start == default)
        {
            return (0, 0, 0);
        }

        var effectiveEntryBtc = start.BtcPrice * (1 + slippagePct);
        var effectiveEntryEth = start.EthPrice * (1 + slippagePct);
        var half = initialCash / 2;
        var btcUnits = effectiveEntryBtc == 0 ? 0 : half / effectiveEntryBtc;
        var ethUnits = effectiveEntryEth == 0 ? 0 : half / effectiveEntryEth;
        var fee = half * feePct * 2;
        var last = candles.OrderBy(c => c.HourUtc).Last();
        var hodlValue = btcUnits * last.BtcPrice + ethUnits * last.EthPrice - fee;

        // DCA and rebalance placeholders reuse hodl for now.
        return (hodlValue, hodlValue, hodlValue);
    }
}
