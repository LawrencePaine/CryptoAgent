import type { LastDecision, Trade } from "../types";

type AgentBiasPanelProps = {
  decision: LastDecision | null;
  lastManualTrade: Trade | null;
};

const formatDecision = (decision: LastDecision) => {
  const action = decision.llmAction?.toString().toUpperCase() ?? "HOLD";
  const asset = decision.llmAsset?.toString().toUpperCase() ?? "NONE";
  const size = decision.llmSizeGbp ?? 0;
  if (action === "HOLD" || asset === "NONE" || size <= 0) {
    return "No trade suggested";
  }
  return `${action} £${size.toFixed(2)} ${asset}`;
};

export function AgentBiasPanel({ decision, lastManualTrade }: AgentBiasPanelProps) {
  const manualLabel = lastManualTrade
    ? `${lastManualTrade.action.toString().toUpperCase()} £${lastManualTrade.sizeGbp.toFixed(2)} ${lastManualTrade.asset
        .toString()
        .toUpperCase()}`
    : "No manual trades placed yet.";

  return (
    <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg space-y-4">
      <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Agent Bias vs Your Action</h2>
      <div className="space-y-3 text-sm">
        <div className="bg-crypto-dark/50 border border-white/5 rounded-xl p-4">
          <div className="text-xs text-gray-500 uppercase">Agent Bias (this tick)</div>
          <div className="text-white font-semibold mt-1">{decision ? formatDecision(decision) : "No trade suggested"}</div>
          {decision?.rationaleShort && (
            <p className="text-gray-400 text-xs mt-2">{decision.rationaleShort}</p>
          )}
        </div>
        <div className="bg-crypto-dark/50 border border-white/5 rounded-xl p-4">
          <div className="text-xs text-gray-500 uppercase">Your Action (last manual trade)</div>
          <div className="text-white font-semibold mt-1">{manualLabel}</div>
          {lastManualTrade ? (
            <p className="text-gray-400 text-xs mt-2">
              {new Date(lastManualTrade.timestampUtc).toLocaleString()} • Outcome pending
            </p>
          ) : (
            <p className="text-gray-500 text-xs mt-2">No manual trades placed yet.</p>
          )}
        </div>
      </div>
    </div>
  );
}
