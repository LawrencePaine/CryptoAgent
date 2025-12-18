import type { Trade } from "../types";

export function RecentTradesTable({ trades }: { trades: Trade[] }) {
    return (
        <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg mt-6 overflow-hidden">
            <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-6">Recent Trades</h2>
            <div className="overflow-x-auto">
                <table className="min-w-full text-sm">
                    <thead>
                        <tr className="border-b border-white/5 text-gray-500">
                            <th className="p-4 text-left font-medium">Time</th>
                            <th className="p-4 text-left font-medium">Asset</th>
                            <th className="p-4 text-left font-medium">Action</th>
                            <th className="p-4 text-left font-medium">Units</th>
                            <th className="p-4 text-left font-medium">Size (£)</th>
                            <th className="p-4 text-left font-medium">Price (£)</th>
                            <th className="p-4 text-left font-medium">Mode</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">
                        {trades.map((t, i) => (
                            <tr key={i} className="hover:bg-white/5 transition-colors group">
                                <td className="p-4 text-gray-300 font-mono text-xs">{new Date(t.timestampUtc).toLocaleString()}</td>
                                <td className="p-4 font-bold text-white">{t.asset}</td>
                                <td className="p-4">
                                    <span className={`px-2 py-1 rounded text-xs font-bold ${t.action.toUpperCase() === 'BUY'
                                            ? 'bg-green-500/10 text-green-400 border border-green-500/20'
                                            : 'bg-red-500/10 text-red-400 border border-red-500/20'
                                        }`}>
                                        {t.action}
                                    </span>
                                </td>
                                <td className="p-4 text-gray-300 font-mono">{t.assetAmount.toFixed(8)}</td>
                                <td className="p-4 text-gray-300 font-mono">£{t.sizeGbp.toFixed(2)}</td>
                                <td className="p-4 text-gray-300 font-mono">£{t.priceGbp.toLocaleString()}</td>
                                <td className="p-4">
                                    <span className="text-xs text-gray-500 border border-gray-700 px-2 py-0.5 rounded">{t.mode}</span>
                                </td>
                            </tr>
                        ))}
                        {trades.length === 0 && (
                            <tr>
                                <td colSpan={7} className="p-8 text-center text-gray-500 italic">No trades recorded in the neural log.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}
