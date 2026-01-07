import type { ExogenousDecisionTrace } from "../types";

type ExogenousDecisionTracePanelProps = {
  trace: ExogenousDecisionTrace;
};

const formatDelta = (value: number) => {
  if (Number.isNaN(value)) {
    return "0.00";
  }
  return value >= 0 ? `+${value.toFixed(2)}` : value.toFixed(2);
};

const badgeTone = (value: string) => {
  switch (value.toUpperCase()) {
    case "ALIGNED":
    case "SUPPORTIVE":
      return "bg-emerald-500/10 text-emerald-200 border-emerald-400/30";
    case "MISALIGNED":
    case "ADVERSE":
      return "bg-rose-500/10 text-rose-200 border-rose-400/30";
    default:
      return "bg-indigo-500/10 text-indigo-200 border-indigo-400/30";
  }
};

export function ExogenousDecisionTracePanel({ trace }: ExogenousDecisionTracePanelProps) {
  const tickLabel = new Date(trace.tickUtc).toLocaleString();

  return (
    <div className="bg-gradient-to-r from-indigo-900/40 to-purple-900/40 border border-indigo-500/30 p-6 rounded-2xl shadow-lg backdrop-blur-sm">
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-3 mb-4">
        <div>
          <p className="text-indigo-300 font-bold text-xs uppercase tracking-wider">Exogenous Decision Trace</p>
          <p className="text-indigo-100 text-sm font-mono">Tick: {tickLabel}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {Object.entries(trace.marketAlignment).map(([asset, alignment]) => (
            <span
              key={`${asset}-${alignment}`}
              className={`px-3 py-1 rounded-full text-xs font-semibold border ${badgeTone(alignment)}`}
            >
              {asset.toUpperCase()}: {alignment}
            </span>
          ))}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
        <div className="space-y-3">
          <h4 className="text-indigo-200 text-sm font-semibold uppercase tracking-wide">Theme Summary</h4>
          <div className="flex flex-wrap gap-2">
            {trace.themes.map((theme) => (
              <div
                key={theme.theme}
                className="px-3 py-2 rounded-xl border border-indigo-400/20 bg-indigo-500/10 text-indigo-100 text-xs"
              >
                <div className="font-semibold">{theme.theme.replace("_", " ")}</div>
                <div className="opacity-80">
                  Strength {theme.strength} • {theme.direction} • Conflict {theme.conflict.toFixed(2)}
                </div>
              </div>
            ))}
            {trace.themes.length === 0 && (
              <p className="text-indigo-200/70 text-xs">No theme scores recorded for this tick.</p>
            )}
          </div>
        </div>

        <div className="space-y-3">
          <h4 className="text-indigo-200 text-sm font-semibold uppercase tracking-wide">Modifiers</h4>
          <div className="flex flex-wrap gap-2">
            <span className="px-3 py-2 rounded-xl border border-indigo-400/20 bg-indigo-500/10 text-xs text-indigo-100">
              Abstain {formatDelta(trace.modifiers.abstainModifier)}
            </span>
            <span className="px-3 py-2 rounded-xl border border-indigo-400/20 bg-indigo-500/10 text-xs text-indigo-100">
              Threshold {formatDelta(trace.modifiers.confidenceThresholdModifier)}
            </span>
            {trace.modifiers.positionSizeModifier !== null && trace.modifiers.positionSizeModifier !== undefined && (
              <span className="px-3 py-2 rounded-xl border border-indigo-400/20 bg-indigo-500/10 text-xs text-indigo-100">
                Size {trace.modifiers.positionSizeModifier.toFixed(2)}x
              </span>
            )}
          </div>

          <h4 className="text-indigo-200 text-sm font-semibold uppercase tracking-wide mt-4">Why</h4>
          <ul className="list-disc list-inside text-indigo-100/90 text-sm space-y-1">
            {trace.gatingReasons.length > 0 ? (
              trace.gatingReasons.map((reason) => <li key={reason}>{reason}</li>)
            ) : (
              <li>No gating reasons logged for this tick.</li>
            )}
          </ul>
        </div>
      </div>

      <div className="mt-5 space-y-4">
        <details className="group">
          <summary className="cursor-pointer text-indigo-200 font-semibold text-sm flex items-center justify-between">
            <span>Top Narratives</span>
            <span className="text-xs text-indigo-300 group-open:hidden">Show</span>
            <span className="text-xs text-indigo-300 hidden group-open:inline">Hide</span>
          </summary>
          <div className="mt-3 space-y-2">
            {trace.topNarratives.length > 0 ? (
              trace.topNarratives.map((narrative) => (
                <div
                  key={narrative.id}
                  className="p-3 rounded-xl border border-indigo-400/20 bg-indigo-500/10 text-indigo-100"
                >
                  <div className="flex flex-wrap items-center gap-2 text-xs">
                    <span className="font-semibold">{narrative.label}</span>
                    <span className={`px-2 py-0.5 rounded-full border ${badgeTone(narrative.direction)}`}>
                      {narrative.direction}
                    </span>
                    <span className="text-indigo-200/70">{narrative.theme.replace("_", " ")}</span>
                    <span className="text-indigo-200/70">{narrative.horizon}</span>
                  </div>
                  <div className="text-xs text-indigo-200/70 mt-1">
                    Score {narrative.stateScore} • Items {narrative.itemCount} • Updated{" "}
                    {new Date(narrative.lastUpdatedUtc).toLocaleString()}
                  </div>
                </div>
              ))
            ) : (
              <p className="text-indigo-200/70 text-sm">No narratives captured for this tick.</p>
            )}
          </div>
        </details>

        <details className="group">
          <summary className="cursor-pointer text-indigo-200 font-semibold text-sm flex items-center justify-between">
            <span>Top Items</span>
            <span className="text-xs text-indigo-300 group-open:hidden">Show</span>
            <span className="text-xs text-indigo-300 hidden group-open:inline">Hide</span>
          </summary>
          <div className="mt-3 space-y-2">
            {trace.topItems.length > 0 ? (
              trace.topItems.map((item) => (
                <div
                  key={item.id}
                  className="p-3 rounded-xl border border-indigo-400/20 bg-indigo-500/10 text-indigo-100"
                >
                  <div className="flex flex-wrap items-center gap-2 text-xs">
                    <a
                      href={item.url}
                      target="_blank"
                      rel="noreferrer"
                      className="font-semibold text-indigo-100 hover:text-white underline underline-offset-2"
                    >
                      {item.title}
                    </a>
                    <span className="text-indigo-200/70">{item.sourceId}</span>
                    <span className="text-indigo-200/70">{item.theme.replace("_", " ")}</span>
                    <span className={`px-2 py-0.5 rounded-full border ${badgeTone(item.directionalBias)}`}>
                      {item.directionalBias}
                    </span>
                  </div>
                  <div className="text-xs text-indigo-200/70 mt-1">
                    {item.impactHorizon} • {new Date(item.publishedAtUtc).toLocaleString()} • Confidence{" "}
                    {(item.confidenceScore * 100).toFixed(0)}%{" "}
                    {item.contributionWeight !== null && item.contributionWeight !== undefined && (
                      <span>• Weight {item.contributionWeight.toFixed(2)}</span>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <p className="text-indigo-200/70 text-sm">No items captured for this tick.</p>
            )}
          </div>
        </details>
      </div>
    </div>
  );
}
