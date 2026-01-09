import { useMemo, useState } from "react";
import type { MarketSnapshot, PortfolioDto } from "../types";

type ManualTradeTicketProps = {
  market: MarketSnapshot;
  portfolio: PortfolioDto;
  onExecute: (payload: { asset: "BTC" | "ETH"; action: "BUY" | "SELL"; sizeGbp: number }) => Promise<void>;
  disabled?: boolean;
  error?: string | null;
};

const FEE_PCT = 0.005;
const SLIPPAGE_PCT = 0.001;

export function ManualTradeTicket({ market, portfolio, onExecute, disabled, error }: ManualTradeTicketProps) {
  const [asset, setAsset] = useState<"BTC" | "ETH">("BTC");
  const [action, setAction] = useState<"BUY" | "SELL">("BUY");
  const [sizeGbp, setSizeGbp] = useState(10);
  const [submitting, setSubmitting] = useState(false);

  const price = asset === "BTC" ? market.btcPriceGbp : market.ethPriceGbp;
  const effectivePrice = action === "BUY" ? price * (1 + SLIPPAGE_PCT) : price * (1 - SLIPPAGE_PCT);
  const feeGbp = sizeGbp * FEE_PCT;
  const units = effectivePrice > 0 ? sizeGbp / effectivePrice : 0;
  const holdings = asset === "BTC" ? portfolio.btc.amount : portfolio.eth.amount;
  const maxSellValue = holdings * effectivePrice;

  const resultingCash = useMemo(() => {
    if (action === "BUY") {
      return portfolio.cashGbp - sizeGbp - feeGbp;
    }
    return portfolio.cashGbp + sizeGbp - feeGbp;
  }, [action, feeGbp, portfolio.cashGbp, sizeGbp]);

  const canSubmit = sizeGbp > 0 && !submitting && !disabled;

  const handleSubmit = async () => {
    if (!canSubmit) return;
    setSubmitting(true);
    try {
      await onExecute({ asset, action, sizeGbp });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg">
      <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-4">Manual Trade Ticket</h2>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label className="text-xs text-gray-400 uppercase tracking-wide">Asset</label>
          <select
            value={asset}
            onChange={(e) => setAsset(e.target.value as "BTC" | "ETH")}
            className="mt-2 w-full bg-crypto-dark/60 border border-white/10 rounded-lg px-3 py-2 text-white"
          >
            <option value="BTC">BTC</option>
            <option value="ETH">ETH</option>
          </select>
        </div>
        <div>
          <label className="text-xs text-gray-400 uppercase tracking-wide">Action</label>
          <select
            value={action}
            onChange={(e) => setAction(e.target.value as "BUY" | "SELL")}
            className="mt-2 w-full bg-crypto-dark/60 border border-white/10 rounded-lg px-3 py-2 text-white"
          >
            <option value="BUY">BUY</option>
            <option value="SELL">SELL</option>
          </select>
        </div>
        <div>
          <label className="text-xs text-gray-400 uppercase tracking-wide">Size (£)</label>
          <input
            type="number"
            min={0}
            step={0.01}
            value={sizeGbp}
            onChange={(e) => setSizeGbp(Number(e.target.value))}
            className="mt-2 w-full bg-crypto-dark/60 border border-white/10 rounded-lg px-3 py-2 text-white"
          />
        </div>
      </div>

      <div className="mt-4 p-4 rounded-xl bg-crypto-dark/50 border border-white/5 text-sm text-gray-300 space-y-2">
        <div className="flex justify-between">
          <span>Estimated units</span>
          <span className="font-mono text-white">{units.toFixed(8)} {asset}</span>
        </div>
        <div className="flex justify-between">
          <span>Estimated fee</span>
          <span className="font-mono text-white">£{feeGbp.toFixed(2)}</span>
        </div>
        <div className="flex justify-between">
          <span>Resulting cash</span>
          <span className={`font-mono ${resultingCash >= 0 ? "text-white" : "text-red-400"}`}>£{resultingCash.toFixed(2)}</span>
        </div>
        <div className="text-xs text-gray-500">Preview uses 0.5% fee + 0.1% slippage.</div>
        {action === "SELL" && sizeGbp > maxSellValue && (
          <div className="text-xs text-red-400">Sell size exceeds current holdings value (£{maxSellValue.toFixed(2)}).</div>
        )}
      </div>

      {error && (
        <div className="mt-4 text-sm text-red-300 bg-red-900/20 border border-red-500/40 rounded-lg px-3 py-2">
          {error}
        </div>
      )}

      <button
        onClick={handleSubmit}
        disabled={!canSubmit}
        className={`mt-4 w-full px-4 py-3 rounded-xl font-bold text-white transition-all duration-300 ${canSubmit
          ? "bg-gradient-to-r from-green-600 to-emerald-600 hover:from-green-500 hover:to-emerald-500 shadow-lg shadow-green-500/20"
          : "bg-gray-700/80 cursor-not-allowed opacity-60"
          }`}
      >
        {submitting ? "Executing..." : "Execute Paper Trade"}
      </button>
    </div>
  );
}
