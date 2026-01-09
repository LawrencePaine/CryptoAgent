import type { PerformanceCompareResponse } from "../types";

type ComparisonStripProps = {
  data: PerformanceCompareResponse | null;
};

const formatCurrency = (value: number) => `£${value.toFixed(2)}`;

export function ComparisonStrip({ data }: ComparisonStripProps) {
  if (!data) {
    return (
      <div className="bg-crypto-card border border-white/5 rounded-2xl p-4 shadow-lg text-sm text-gray-400">
        Comparison strip will populate once manual trades are placed.
      </div>
    );
  }

  const deltaGbp = data.manual.equity - data.agent.equity;
  const deltaPct = data.agent.equity !== 0 ? deltaGbp / data.agent.equity : 0;

  return (
    <div className="bg-gradient-to-r from-indigo-900/40 to-purple-900/40 border border-indigo-500/30 rounded-2xl p-4 shadow-lg">
      <div className="flex flex-wrap items-center justify-between gap-3 text-sm">
        <div>
          <div className="text-xs text-indigo-300 uppercase">Agent Equity</div>
          <div className="text-white font-mono">{formatCurrency(data.agent.equity)}</div>
        </div>
        <div>
          <div className="text-xs text-indigo-300 uppercase">Manual Equity</div>
          <div className="text-white font-mono">{formatCurrency(data.manual.equity)}</div>
        </div>
        <div>
          <div className="text-xs text-indigo-300 uppercase">Delta</div>
          <div className={`font-mono ${deltaGbp >= 0 ? "text-emerald-300" : "text-rose-300"}`}>
            {deltaGbp >= 0 ? "+" : ""}
            {formatCurrency(deltaGbp)} ({deltaPct >= 0 ? "+" : ""}
            {(deltaPct * 100).toFixed(2)}%)
          </div>
        </div>
      </div>
    </div>
  );
}
