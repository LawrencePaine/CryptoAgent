import type { MarketSnapshot } from "../types";

export function MarketCard({ market }: { market: MarketSnapshot }) {
    const getChangeColor = (val: number) => val >= 0 ? "text-crypto-success" : "text-crypto-danger";
    const getBgColor = (val: number) => val >= 0 ? "bg-crypto-success/10" : "bg-crypto-danger/10";

    return (
        <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg relative overflow-hidden">
            <div className="absolute top-0 right-0 w-32 h-32 bg-purple-500/10 rounded-full blur-3xl -mr-16 -mt-16"></div>

            <div className="flex justify-between items-start mb-6">
                <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider">Live Market</h2>
                <div className="flex items-center space-x-2">
                    <span className="relative flex h-2 w-2">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                        <span className="relative inline-flex rounded-full h-2 w-2 bg-green-500"></span>
                    </span>
                    <span className="text-xs text-gray-500 font-mono">
                        {new Date(market.timestampUtc).toLocaleTimeString()}
                    </span>
                </div>
            </div>

            <div className="space-y-4">
                {/* BTC */}
                <div className="bg-crypto-dark/50 rounded-xl p-4 border border-white/5 hover:border-white/10 transition-colors">
                    <div className="flex justify-between items-center mb-2">
                        <div className="flex items-center space-x-2">
                            <div className="w-8 h-8 rounded-full bg-orange-500/20 flex items-center justify-center text-orange-500 font-bold text-xs">₿</div>
                            <span className="font-bold text-white">Bitcoin</span>
                        </div>
                        <span className="font-mono text-lg text-white">£{market.btcPriceGbp.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between items-center text-sm">
                        <span className="text-gray-500">24h Change</span>
                        <span className={`px-2 py-0.5 rounded ${getBgColor(market.btcChange24hPct)} ${getChangeColor(market.btcChange24hPct)} font-medium`}>
                            {market.btcChange24hPct >= 0 ? "+" : ""}{(market.btcChange24hPct * 100).toFixed(2)}%
                        </span>
                    </div>
                </div>

                {/* ETH */}
                <div className="bg-crypto-dark/50 rounded-xl p-4 border border-white/5 hover:border-white/10 transition-colors">
                    <div className="flex justify-between items-center mb-2">
                        <div className="flex items-center space-x-2">
                            <div className="w-8 h-8 rounded-full bg-blue-500/20 flex items-center justify-center text-blue-500 font-bold text-xs">Ξ</div>
                            <span className="font-bold text-white">Ethereum</span>
                        </div>
                        <span className="font-mono text-lg text-white">£{market.ethPriceGbp.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between items-center text-sm">
                        <span className="text-gray-500">24h Change</span>
                        <span className={`px-2 py-0.5 rounded ${getBgColor(market.ethChange24hPct)} ${getChangeColor(market.ethChange24hPct)} font-medium`}>
                            {market.ethChange24hPct >= 0 ? "+" : ""}{(market.ethChange24hPct * 100).toFixed(2)}%
                        </span>
                    </div>
                </div>
            </div>
        </div>
    );
}
