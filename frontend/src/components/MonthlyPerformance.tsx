import type { MonthlyPerformance } from "../types";

export function MonthlyPerformanceSection({ data }: { data: MonthlyPerformance[] }) {
    return (
        <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg mt-6 lg:mt-0 h-full">
            <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-6">Performance History</h2>
            <div className="space-y-4 max-h-[500px] overflow-y-auto pr-2 custom-scrollbar">
                {data.map((m) => (
                    <div key={`${m.year}-${m.month}`} className="bg-crypto-dark/30 p-4 rounded-xl border border-white/5 hover:bg-white/5 transition-colors">
                        <div className="flex justify-between items-center mb-3">
                            <div className="font-bold text-white text-lg">{m.year}-{m.month.toString().padStart(2, '0')}</div>
                            <div className={`px-2 py-1 rounded text-xs font-bold ${m.netAfterAiGbp >= 0 ? 'bg-green-500/10 text-green-400' : 'bg-red-500/10 text-red-400'}`}>
                                {m.netAfterAiGbp >= 0 ? "PROFIT" : "LOSS"}
                            </div>
                        </div>

                        <div className="space-y-2 text-sm">
                            <div className="flex justify-between text-gray-400">
                                <span>Start / End</span>
                                <span className="font-mono text-gray-300">£{m.startValue.toFixed(0)} → £{m.endValue.toFixed(0)}</span>
                            </div>

                            <div className="flex justify-between items-center pt-2 border-t border-white/5">
                                <span className="text-gray-400">Gross P&L</span>
                                <span className={`font-mono font-medium ${m.pnlGbp >= 0 ? "text-green-400" : "text-red-400"}`}>
                                    {m.pnlGbp >= 0 ? "+" : ""}£{m.pnlGbp.toFixed(2)}
                                </span>
                            </div>
                            <div className="flex justify-between text-gray-500 text-xs">
                                <span>AI Cost</span>
                                <span>-£{m.aiCostGbp.toFixed(2)}</span>
                            </div>
                            <div className="flex justify-between items-center pt-2 border-t border-white/5">
                                <span className="text-gray-300 font-bold">Net Profit</span>
                                <span className={`font-mono font-bold text-lg ${m.netAfterAiGbp >= 0 ? "text-green-400" : "text-red-400"}`}>
                                    {m.netAfterAiGbp >= 0 ? "+" : ""}£{m.netAfterAiGbp.toFixed(2)}
                                </span>
                            </div>
                        </div>
                    </div>
                ))}
                {data.length === 0 && (
                    <div className="text-gray-500 text-center py-8 italic">No performance data available.</div>
                )}
            </div>
        </div>
    );
}
