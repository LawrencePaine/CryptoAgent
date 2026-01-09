import { useMemo, useState } from "react";
import type { Trade } from "../types";

export function TradeLogTable({ trades }: { trades: Trade[] }) {
  const [showAgent, setShowAgent] = useState(true);
  const [showManual, setShowManual] = useState(true);

  const filtered = useMemo(() => {
    return trades.filter((trade) => {
      const book = trade.book?.toString().toUpperCase() ?? "AGENT";
      if (book === "AGENT" && !showAgent) return false;
      if (book === "MANUAL" && !showManual) return false;
      return true;
    });
  }, [showAgent, showManual, trades]);

  return (
    <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg mt-6 overflow-hidden">
      <div className="flex flex-wrap items-center justify-between gap-4 mb-4">
        <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Trade Log</h2>
        <div className="flex items-center gap-3 text-xs">
          <label className="flex items-center gap-2 text-gray-300">
            <input type="checkbox" checked={showAgent} onChange={(e) => setShowAgent(e.target.checked)} />
            Agent
          </label>
          <label className="flex items-center gap-2 text-gray-300">
            <input type="checkbox" checked={showManual} onChange={(e) => setShowManual(e.target.checked)} />
            Manual
          </label>
        </div>
      </div>
      <div className="overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead>
            <tr className="border-b border-white/5 text-gray-500">
              <th className="p-4 text-left font-medium">Time</th>
              <th className="p-4 text-left font-medium">Book</th>
              <th className="p-4 text-left font-medium">Asset</th>
              <th className="p-4 text-left font-medium">Action</th>
              <th className="p-4 text-left font-medium">Units</th>
              <th className="p-4 text-left font-medium">Size (£)</th>
              <th className="p-4 text-left font-medium">Price (£)</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-white/5">
            {filtered.map((t, i) => {
              const book = t.book?.toString().toUpperCase() ?? "AGENT";
              return (
                <tr key={i} className="hover:bg-white/5 transition-colors group">
                  <td className="p-4 text-gray-300 font-mono text-xs">{new Date(t.timestampUtc).toLocaleString()}</td>
                  <td className="p-4 text-xs text-gray-400">{book}</td>
                  <td className="p-4 font-bold text-white">{t.asset}</td>
                  <td className="p-4">
                    <span className={`px-2 py-1 rounded text-xs font-bold ${t.action.toUpperCase() === "BUY"
                        ? "bg-green-500/10 text-green-400 border border-green-500/20"
                        : "bg-red-500/10 text-red-400 border border-red-500/20"
                      }`}>
                      {t.action}
                    </span>
                  </td>
                  <td className="p-4 text-gray-300 font-mono">{t.assetAmount.toFixed(8)}</td>
                  <td className="p-4 text-gray-300 font-mono">£{t.sizeGbp.toFixed(2)}</td>
                  <td className="p-4 text-gray-300 font-mono">£{t.priceGbp.toLocaleString()}</td>
                </tr>
              );
            })}
            {filtered.length === 0 && (
              <tr>
                <td colSpan={7} className="p-8 text-center text-gray-500 italic">
                  No trades to show.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
