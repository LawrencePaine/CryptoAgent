import { useState } from "react";
import type { LastDecision } from "../types";

export function LastDecisionCard({ decision }: { decision: LastDecision | null }) {
    if (!decision) {
        return (
            <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg flex items-center justify-center min-h-[300px]">
                <div className="text-center">
                    <div className="w-16 h-16 bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-4">
                        <span className="text-2xl">🤖</span>
                    </div>
                    <h2 className="text-gray-400 font-medium">No decisions recorded</h2>
                    <p className="text-gray-600 text-sm mt-2">The agent is waiting for its first run.</p>
                </div>
            </div>
        );
    }

    const action = decision.finalAction.toUpperCase();
    const isBuy = action === "BUY";
    const isSell = action === "SELL";
    const isHold = action === "HOLD";

    let colorClass = "text-gray-400 bg-gray-500/10 border-gray-500/20";
    if (isBuy) colorClass = "text-green-400 bg-green-500/10 border-green-500/20";
    if (isSell) colorClass = "text-red-400 bg-red-500/10 border-red-500/20";
    if (isHold) colorClass = "text-blue-400 bg-blue-500/10 border-blue-500/20";

    const [hoverRect, setHoverRect] = useState<DOMRect | null>(null);

    const handleMouseEnter = (e: React.MouseEvent) => {
        setHoverRect(e.currentTarget.getBoundingClientRect());
    };

    const handleMouseLeave = () => {
        setHoverRect(null);
    };

    return (
        <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg relative overflow-hidden flex flex-col h-full">
            <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-500/10 rounded-full blur-3xl -mr-16 -mt-16"></div>

            <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-6">Latest AI Decision</h2>

            <div className="flex-1 flex flex-col items-center justify-center mb-6">
                <div className={`px-6 py-3 rounded-2xl border-2 ${colorClass} backdrop-blur-sm mb-4`}>
                    <span className="text-3xl font-bold tracking-wider">{action}</span>
                </div>

                {decision.finalAsset !== "None" && (
                    <div className="text-xl text-white font-medium">
                        {decision.finalAsset}
                        {decision.executed && <span className="text-gray-500 mx-2">•</span>}
                        {decision.executed && <span className="font-mono text-gray-300">£{decision.finalSizeGbp.toFixed(2)}</span>}
                    </div>
                )}
            </div>

            <div className="bg-crypto-dark/50 rounded-xl p-4 border border-white/5 space-y-3">
                <div>
                    <span className="text-xs text-gray-500 uppercase tracking-wider block mb-1">Rationale</span>
                    <p
                        className="text-gray-300 text-sm leading-relaxed italic cursor-help border-b border-dotted border-gray-600 hover:text-white transition-colors inline-block"
                        onMouseEnter={handleMouseEnter}
                        onMouseLeave={handleMouseLeave}
                    >
                        "{decision.rationaleShort}"
                    </p>
                </div>

                {decision.riskReason && (
                    <div className="pt-3 border-t border-white/5">
                        <span className="text-xs text-orange-400 uppercase tracking-wider block mb-1">Risk Assessment</span>
                        <p className="text-orange-200/80 text-sm">{decision.riskReason}</p>
                    </div>
                )}
            </div>

            <div className="mt-4 text-center">
                <span className="text-xs text-gray-600 font-mono">
                    ID: {new Date(decision.timestampUtc).getTime().toString(36).toUpperCase()} • {new Date(decision.timestampUtc).toLocaleString()}
                </span>
            </div>

            {/* Hover Card Portal */}
            {hoverRect && (
                <div
                    className="fixed z-50 w-80 bg-gray-900 border border-white/10 rounded-xl shadow-2xl p-4 text-sm pointer-events-none animate-in fade-in zoom-in-95 duration-150"
                    style={{
                        top: hoverRect.bottom + 8,
                        left: Math.min(hoverRect.left, window.innerWidth - 340), // Prevent going off-screen
                    }}
                >
                    <div className="flex items-center gap-2 mb-2 pb-2 border-b border-white/5">
                        <div className={`w-2 h-2 rounded-full ${decision.llmAction === 'Buy' ? 'bg-green-500' :
                            decision.llmAction === 'Sell' ? 'bg-red-500' : 'bg-gray-500'
                            }`} />
                        <span className="font-semibold text-gray-200">Rationale</span>
                    </div>
                    <p className="text-gray-300 leading-relaxed">
                        {decision.rationaleDetailed || decision.rationaleShort}
                    </p>
                    {decision.rationaleDetailed && (
                        <div className="mt-3 text-xs text-gray-500 font-mono">
                            Detailed reasoning from LLM
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}
