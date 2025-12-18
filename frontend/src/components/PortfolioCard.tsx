import type { PortfolioDto } from "../types";

export function PortfolioCard({ portfolio }: { portfolio: PortfolioDto }) {
    return (
        <div className="bg-crypto-card border border-white/5 rounded-2xl p-6 shadow-lg hover:shadow-xl transition-shadow duration-300 relative overflow-hidden group">
            <div className="absolute top-0 right-0 w-32 h-32 bg-blue-500/10 rounded-full blur-3xl -mr-16 -mt-16 transition-all group-hover:bg-blue-500/20"></div>

            <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-1">Portfolio Value</h2>
            <div className="text-4xl font-bold text-white mb-6 tracking-tight">
                £{portfolio.totalValueGbp.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </div>

            <div className="space-y-4">
                {/* Cash */}
                <div className="bg-crypto-dark/50 rounded-lg p-3 border border-white/5">
                    <div className="flex justify-between items-center mb-1">
                        <span className="text-gray-300 font-medium">Cash</span>
                        <span className="text-white font-mono">£{portfolio.cashGbp.toLocaleString(undefined, { minimumFractionDigits: 2 })}</span>
                    </div>
                    <div className="w-full bg-gray-700 rounded-full h-1.5">
                        <div className="bg-green-500 h-1.5 rounded-full" style={{ width: `${portfolio.cashAllocationPct * 100}%` }}></div>
                    </div>
                    <div className="text-right text-xs text-gray-500 mt-1">{(portfolio.cashAllocationPct * 100).toFixed(1)}%</div>
                </div>

                {/* Vault */}
                <div className="flex justify-between items-center text-sm p-2">
                    <span className="text-gray-400">Vault (Banked)</span>
                    <span className="text-green-400 font-mono font-medium">+£{portfolio.vaultGbp.toLocaleString(undefined, { minimumFractionDigits: 2 })}</span>
                </div>

                {/* Assets */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                    {[{
                        label: "BTC",
                        color: "text-orange-400",
                        data: portfolio.btc
                    }, {
                        label: "ETH",
                        color: "text-blue-400",
                        data: portfolio.eth
                    }].map((asset) => (
                        <div key={asset.label} className="bg-crypto-dark/50 rounded-lg p-3 border border-white/5">
                            <div className="flex justify-between items-start mb-1">
                                <div>
                                    <div className={`${asset.color} font-bold`}>{asset.label}</div>
                                    <div className="text-xs text-gray-500">Allocation {(asset.data.allocationPct * 100).toFixed(1)}%</div>
                                </div>
                                <div className="text-right">
                                    <div className="text-xs text-gray-500 uppercase">Holding</div>
                                    <div className="text-white font-mono text-sm">{asset.data.amount.toFixed(8)} {asset.label}</div>
                                </div>
                            </div>

                            <div className="text-gray-300 text-sm">Cost: £{asset.data.costBasisGbp.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
                            <div className="text-gray-300 text-sm">Value now: £{asset.data.currentValueGbp.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
                            <div className={`text-sm font-semibold ${asset.data.unrealisedPnlGbp >= 0 ? 'text-green-400' : 'text-red-400'}`}>
                                Unrealised: {asset.data.unrealisedPnlGbp >= 0 ? '+' : ''}£{asset.data.unrealisedPnlGbp.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                                {` (${(asset.data.unrealisedPnlPct * 100).toFixed(2)}%)`}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}
