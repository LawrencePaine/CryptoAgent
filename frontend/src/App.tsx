import { useEffect, useState } from "react";
import { api } from "./api";
import type { DashboardResponse, MonthlyPerformance } from "./types";
import { PortfolioCard } from "./components/PortfolioCard";
import { MarketCard } from "./components/MarketCard";
import { LastDecisionCard } from "./components/LastDecisionCard";
import { RecentTradesTable } from "./components/RecentTradesTable";
import { MonthlyPerformanceSection } from "./components/MonthlyPerformance";

function App() {
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null);
  const [performance, setPerformance] = useState<MonthlyPerformance[]>([]);
  const [loading, setLoading] = useState(false);
  const [running, setRunning] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      const db = await api.getDashboard();
      setDashboard(db);
      const perf = await api.getMonthlyPerformance();
      setPerformance(perf);
    } catch (err) {
      setError("Failed to fetch data. Is the backend running?");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleRunAgent = async () => {
    setRunning(true);
    try {
      const updatedDb = await api.runAgent();
      setDashboard(updatedDb);
      // Refresh performance too
      const perf = await api.getMonthlyPerformance();
      setPerformance(perf);
    } catch (err) {
      setError("Failed to run agent.");
      console.error(err);
    } finally {
      setRunning(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  if (!dashboard && loading) {
    return (
      <div className="min-h-screen flex items-center justify-center text-white">
        <div className="animate-pulse text-xl font-mono text-crypto-accent">Loading Neural Interface...</div>
      </div>
    );
  }

  if (error && !dashboard) {
    return (
      <div className="min-h-screen flex items-center justify-center p-8">
        <div className="bg-crypto-card border border-crypto-danger/50 p-8 rounded-xl max-w-md w-full text-center shadow-2xl shadow-crypto-danger/20">
          <h1 className="text-2xl font-bold mb-4 text-crypto-danger">System Error</h1>
          <p className="text-gray-300 mb-6">{error}</p>
          <button
            onClick={fetchData}
            className="px-6 py-2 bg-crypto-danger hover:bg-red-600 text-white rounded-lg transition-all duration-200 shadow-lg shadow-red-900/20 font-medium"
          >
            Retry Connection
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen text-gray-100 font-sans selection:bg-crypto-accent/30">
      <div className="max-w-7xl mx-auto p-6">
        {/* Header */}
        <header className="flex flex-col md:flex-row justify-between items-center mb-10 bg-crypto-card/50 backdrop-blur-md border border-white/5 p-6 rounded-2xl shadow-xl">
          <div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-400 to-purple-500 bg-clip-text text-transparent">
              CryptoAgent<span className="text-white/20">.ai</span>
            </h1>
            {dashboard && (
              <p className="text-gray-400 mt-2 text-sm font-mono">
                <span className="w-2 h-2 inline-block rounded-full bg-crypto-success mr-2 animate-pulse"></span>
                System Online • Last Sync: {new Date(dashboard.market.timestampUtc).toLocaleTimeString()}
              </p>
            )}
          </div>

          <div className="flex items-center space-x-8 mt-6 md:mt-0">
            {dashboard && (
              <div className="text-right">
                <div className="text-xs text-gray-400 uppercase tracking-wider mb-1">Total Equity</div>
                <div className="text-3xl font-bold text-white tracking-tight">
                  £{dashboard.portfolio.totalValueGbp.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                </div>
              </div>
            )}

            <button
              onClick={handleRunAgent}
              disabled={running}
              className={`relative overflow-hidden px-8 py-3 rounded-xl font-bold text-white transition-all duration-300 transform hover:scale-105 active:scale-95 ${running
                ? "bg-gray-700 cursor-not-allowed opacity-50"
                : "bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 shadow-lg shadow-blue-500/25 border border-blue-400/20"
                }`}
            >
              <span className="relative z-10 flex items-center gap-2">
                {running ? (
                  <>
                    <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Processing...
                  </>
                ) : (
                  "Run Agent"
                )}
              </span>
            </button>
          </div>
        </header>

        {error && (
          <div className="bg-red-900/20 border border-red-500/50 text-red-200 px-4 py-3 rounded-lg mb-8 backdrop-blur-sm">
            {error}
          </div>
        )}

        {dashboard && (
          <main className="space-y-8 animate-fade-in">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <PortfolioCard portfolio={dashboard.portfolio} />
              <MarketCard market={dashboard.market} />
              <LastDecisionCard decision={dashboard.lastDecision} />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <div className="lg:col-span-2">
                <RecentTradesTable trades={dashboard.recentTrades} />
              </div>
              <div className="lg:col-span-1">
                <MonthlyPerformanceSection data={performance} />
              </div>
            </div>
          </main>
        )}
      </div>
    </div>
  );
}

export default App;
