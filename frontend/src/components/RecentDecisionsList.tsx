import { useState } from "react";
import type { LastDecision } from "../types";

export function RecentDecisionsList({ decisions }: { decisions: LastDecision[] }) {
    const [hoveredDecision, setHoveredDecision] = useState<{ id: number, rect: DOMRect, decision: LastDecision } | null>(null);

    const handleMouseEnter = (e: React.MouseEvent, decision: LastDecision, index: number) => {
        const rect = e.currentTarget.getBoundingClientRect();
        setHoveredDecision({ id: index, rect, decision });
    };

    const handleMouseLeave = () => {
        setHoveredDecision(null);
    };

    return (
        <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg overflow-hidden h-full relative">
            <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-6">Recent Decision Log</h2>
            <div className="overflow-x-auto">
                <table className="min-w-full text-sm">
                    <thead>
                        <tr className="border-b border-white/5 text-gray-500">
                            <th className="p-4 text-left font-medium">Time</th>
                            <th className="p-4 text-left font-medium">Suggestion</th>
                            <th className="p-4 text-left font-medium">Rationale</th>
                            <th className="p-4 text-left font-medium">Outcome</th>
                            <th className="p-4 text-left font-medium">Risk Note</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">
                        {decisions.map((d, i) => (
                            <tr key={i} className="hover:bg-white/5 transition-colors group">
                                <td className="p-4 text-gray-300 font-mono text-xs whitespace-nowrap">
                                    {new Date(d.timestampUtc).toLocaleString()}
                                </td>
                                <td className="p-4">
                                    <div className="font-medium text-gray-200">
                                        {d.llmAction} {d.llmAsset !== 'None' ? d.llmAsset : ''}
                                    </div>
                                    <div className="text-xs text-gray-500">£{d.llmSizeGbp.toFixed(2)}</div>
                                </td>
                                <td className="p-4 relative">
                                    <div
                                        className="text-gray-400 max-w-xs truncate cursor-help border-b border-dotted border-gray-600 hover:text-white transition-colors"
                                        onMouseEnter={(e) => handleMouseEnter(e, d, i)}
                                        onMouseLeave={handleMouseLeave}
                                    >
                                        {d.rationaleShort}
                                    </div>
                                </td>
                                <td className="p-4">
                                    <span className={`px-2 py-1 rounded text-xs font-bold ${d.executed ? 'bg-green-500/10 text-green-400 border border-green-500/20' : 'bg-gray-500/10 text-gray-400 border border-gray-500/20'
                                        }`}>
                                        {d.executed ? 'EXECUTED' : 'SKIPPED'}
                                    </span>
                                    {d.executed && d.finalAction !== d.llmAction && (
                                        <div className="text-xs text-orange-400 mt-1">Modified</div>
                                    )}
                                </td>
                                <td className="p-4 text-orange-300/80 text-xs">
                                    {d.riskReason}
                                </td>
                            </tr>
                        ))}
                        {decisions.length === 0 && (
                            <tr>
                                <td colSpan={5} className="p-8 text-center text-gray-500 italic">No decisions recorded yet.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>

            {/* Hover Card Portal */}
            {hoveredDecision && (
                <div
                    className="fixed z-50 w-80 bg-gray-900 border border-white/10 rounded-xl shadow-2xl p-4 text-sm pointer-events-none animate-in fade-in zoom-in-95 duration-150"
                    style={{
                        top: hoveredDecision.rect.bottom + 8,
                        left: Math.min(hoveredDecision.rect.left, window.innerWidth - 340), // Prevent going off-screen
                    }}
                >
                    <div className="flex items-center gap-2 mb-2 pb-2 border-b border-white/5">
                        <div className={`w-2 h-2 rounded-full ${hoveredDecision.decision.llmAction === 'Buy' ? 'bg-green-500' :
                            hoveredDecision.decision.llmAction === 'Sell' ? 'bg-red-500' : 'bg-gray-500'
                            }`} />
                        <span className="font-semibold text-gray-200">Rationale</span>
                    </div>
                    <p className="text-gray-300 leading-relaxed">
                        {hoveredDecision.decision.rationaleDetailed || hoveredDecision.decision.rationaleShort}
                    </p>
                    {hoveredDecision.decision.rationaleDetailed && (
                        <div className="mt-3 text-xs text-gray-500 font-mono">
                            Detailed reasoning from LLM
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}
