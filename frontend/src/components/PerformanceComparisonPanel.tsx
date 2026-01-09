import type { PerformanceCompareResponse } from "../types";

export function PerformanceComparisonPanel({ data }: { data: PerformanceCompareResponse | null }) {
  return (
    <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg">
      <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-4">Performance Comparison</h2>
      {!data && <div className="text-gray-500 text-sm">No comparison data yet.</div>}
      {data && (
        <div className="space-y-4 text-sm text-gray-300">
          {(["agent", "manual"] as const).map((key) => {
            const summary = data[key];
            const label = key === "agent" ? "Agent" : "Manual";
            return (
              <div key={key} className="border border-white/5 rounded-xl p-4 bg-crypto-dark/40">
                <div className="flex justify-between items-center mb-2">
                  <span className="text-gray-200 font-semibold">{label}</span>
                  <span className="text-xs text-gray-500">{new Date(data.fromUtc).toLocaleDateString()} → {new Date(data.toUtc).toLocaleDateString()}</span>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div>
                    <div className="text-xs text-gray-500 uppercase">Equity</div>
                    <div className="text-white font-mono">£{summary.equity.toFixed(2)}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 uppercase">Net Profit</div>
                    <div className={`font-mono ${summary.netProfit >= 0 ? "text-green-400" : "text-red-400"}`}>
                      {summary.netProfit >= 0 ? "+" : ""}£{summary.netProfit.toFixed(2)} ({(summary.netProfitPct * 100).toFixed(2)}%)
                    </div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 uppercase">Fees</div>
                    <div className="text-white font-mono">£{summary.fees.toFixed(2)}</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 uppercase">Max Drawdown</div>
                    <div className="text-white font-mono">{(summary.maxDrawdownPct * 100).toFixed(2)}%</div>
                  </div>
                  <div>
                    <div className="text-xs text-gray-500 uppercase">Trades</div>
                    <div className="text-white font-mono">{summary.tradeCount}</div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
