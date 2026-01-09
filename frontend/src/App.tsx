import { useEffect, useMemo, useState } from "react";
import { api } from "./api";
import type {
  DashboardResponse,
  ExogenousRefreshJobStatus,
  ManualTradeRequest,
  MonthlyPerformance,
  PerformanceCompareResponse,
  PortfolioDto,
  Trade,
} from "./types";
import { PortfolioCard } from "./components/PortfolioCard";
import { MarketCard } from "./components/MarketCard";
import { LastDecisionCard } from "./components/LastDecisionCard";
import { RecentTradesTable } from "./components/RecentTradesTable";
import { RecentDecisionsList } from "./components/RecentDecisionsList";
import { MonthlyPerformanceSection } from "./components/MonthlyPerformance";
import { ExogenousDecisionTracePanel } from "./components/ExogenousDecisionTracePanel";
import { ManualTradeTicket } from "./components/ManualTradeTicket";
import { PerformanceComparisonPanel } from "./components/PerformanceComparisonPanel";
import { TradeLogTable } from "./components/TradeLogTable";

function App() {
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null);
  const [performance, setPerformance] = useState<MonthlyPerformance[]>([]);
  const [loading, setLoading] = useState(false);
  const [running, setRunning] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<"dashboard" | "simulator">("dashboard");
  const [refreshStatus, setRefreshStatus] = useState<ExogenousRefreshJobStatus | null>(null);
  const [refreshError, setRefreshError] = useState<string | null>(null);
  const [manualPortfolio, setManualPortfolio] = useState<PortfolioDto | null>(null);
  const [manualTrades, setManualTrades] = useState<Trade[]>([]);
  const [agentTrades, setAgentTrades] = useState<Trade[]>([]);
  const [comparison, setComparison] = useState<PerformanceCompareResponse | null>(null);
  const [manualError, setManualError] = useState<string | null>(null);
  const [simulatorLoading, setSimulatorLoading] = useState(false);

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

  const fetchSimulatorData = async () => {
    setSimulatorLoading(true);
    setManualError(null);
    try {
      const [portfolio, manualLog, agentLog, compare] = await Promise.all([
        api.getManualPortfolio(),
        api.getTrades("MANUAL", 50),
        api.getTrades("AGENT", 50),
        api.getPerformanceCompare(),
      ]);
      setManualPortfolio(portfolio);
      setManualTrades(manualLog);
      setAgentTrades(agentLog);
      setComparison(compare);
    } catch (err) {
      setManualError("Failed to load simulator data.");
      console.error(err);
    } finally {
      setSimulatorLoading(false);
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

  const handleRefreshContext = async () => {
    setRefreshError(null);
    setRefreshStatus(null);
    try {
      const result = await api.refreshExogenousContext();
      const poll = async () => {
        const status = await api.getRefreshStatus(result.jobId);
        setRefreshStatus(status);
        if (status.status === "Queued" || status.status === "Running") {
          setTimeout(poll, 2000);
        } else if (status.status === "Succeeded") {
          fetchData();
        }
      };
      poll();
    } catch (err) {
      setRefreshError(err instanceof Error ? err.message : "Failed to refresh context.");
      console.error(err);
    }
  };

  const handleManualTrade = async (payload: ManualTradeRequest) => {
    setManualError(null);
    try {
      const result = await api.executeManualTrade(payload);
      setManualPortfolio(result.portfolio);
      const [manualLog, agentLog, compare] = await Promise.all([
        api.getTrades("MANUAL", 50),
        api.getTrades("AGENT", 50),
        api.getPerformanceCompare(),
      ]);
      setManualTrades(manualLog);
      setAgentTrades(agentLog);
      setComparison(compare);
      if (dashboard) {
        setDashboard({ ...dashboard, market: result.market });
      }
    } catch (err) {
      setManualError(err instanceof Error ? err.message : "Failed to execute manual trade.");
      console.error(err);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    const syncFromHash = () => {
      if (window.location.hash.includes("simulator")) {
        setViewMode("simulator");
      } else {
        setViewMode("dashboard");
      }
    };
    syncFromHash();
    window.addEventListener("hashchange", syncFromHash);
    return () => window.removeEventListener("hashchange", syncFromHash);
  }, []);

  useEffect(() => {
    if (viewMode === "simulator") {
      if (!dashboard) {
        fetchData();
      }
      fetchSimulatorData();
    }
  }, [viewMode]);

  const combinedTrades = useMemo(() => {
    return [...manualTrades, ...agentTrades].sort(
      (a, b) => new Date(b.timestampUtc).getTime() - new Date(a.timestampUtc).getTime()
    );
  }, [agentTrades, manualTrades]);

  const marketStale = useMemo(() => {
    if (!dashboard) return false;
    const diffMs = Date.now() - new Date(dashboard.market.timestampUtc).getTime();
    return diffMs > 2 * 60 * 1000;
  }, [dashboard]);

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
        <div className="flex flex-wrap items-center gap-3 mb-6">
          <button
            onClick={() => {
              window.location.hash = "";
              setViewMode("dashboard");
            }}
            className={`px-4 py-2 rounded-full text-sm font-semibold transition-all ${viewMode === "dashboard"
              ? "bg-blue-600 text-white shadow-lg shadow-blue-500/30"
              : "bg-crypto-card text-gray-300 border border-white/10"
              }`}
          >
            Agent Dashboard
          </button>
          <button
            onClick={() => {
              window.location.hash = "#/simulator";
              setViewMode("simulator");
            }}
            className={`px-4 py-2 rounded-full text-sm font-semibold transition-all ${viewMode === "simulator"
              ? "bg-purple-600 text-white shadow-lg shadow-purple-500/30"
              : "bg-crypto-card text-gray-300 border border-white/10"
              }`}
          >
            Trader Simulator
          </button>
        </div>
        {/* Header */}
        <header className="flex flex-col md:flex-row justify-between items-center mb-10 bg-crypto-card/50 backdrop-blur-md border border-white/5 p-6 rounded-2xl shadow-xl">
          <div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-400 to-purple-500 bg-clip-text text-transparent">
              CryptoAgent<span className="text-white/20">.ai</span>
            </h1>
            {dashboard && (
              <p className="text-gray-400 mt-2 text-sm font-mono">
                <span className="w-2 h-2 inline-block rounded-full bg-crypto-success mr-2 animate-pulse"></span>
                System Online • Market Sync: {new Date(dashboard.market.timestampUtc).toLocaleTimeString()}
                {dashboard.exogenousLastSyncUtc && (
                  <> • Context Sync: {new Date(dashboard.exogenousLastSyncUtc).toLocaleTimeString()}</>
                )}
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

            <div className="flex flex-col md:flex-row gap-3 items-center">
              <button
                onClick={handleRunAgent}
                disabled={running}
                className={`relative overflow-hidden px-6 py-3 rounded-xl font-bold text-white transition-all duration-300 transform hover:scale-105 active:scale-95 ${running
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
              <div className="flex flex-col items-start">
                <button
                  onClick={handleRefreshContext}
                  className="px-4 py-2 rounded-lg text-sm font-semibold bg-crypto-card border border-white/10 text-white hover:bg-crypto-card/80"
                >
                  Refresh Context
                </button>
                <div className="text-xs text-gray-400 mt-1">
                  Status: {refreshStatus?.status ?? "Idle"}
                </div>
              </div>
            </div>
          </div>
        </header>

        {error && (
          <div className="bg-red-900/20 border border-red-500/50 text-red-200 px-4 py-3 rounded-lg mb-8 backdrop-blur-sm">
            {error}
          </div>
        )}

        {refreshError && (
          <div className="bg-yellow-900/20 border border-yellow-500/50 text-yellow-100 px-4 py-3 rounded-lg mb-8 backdrop-blur-sm">
            Context refresh: {refreshError}
          </div>
        )}

        {dashboard && viewMode === "dashboard" && (
          <main className="space-y-8 animate-fade-in">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <PortfolioCard portfolio={dashboard.portfolio} />
              <MarketCard market={dashboard.market} />
              <LastDecisionCard decision={dashboard.lastDecision} />
            </div>

            {dashboard.positionCommentary && (
              <div className="bg-gradient-to-r from-indigo-900/40 to-purple-900/40 border border-indigo-500/30 p-6 rounded-2xl flex items-start gap-5 backdrop-blur-sm shadow-lg">
                <div className="bg-indigo-500/20 p-3 rounded-xl text-indigo-300 shadow-inner">
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
                <div>
                  <h3 className="text-indigo-300 font-bold text-xs uppercase tracking-wider mb-2">Live Market Commentary</h3>
                  <p className="text-indigo-50 text-lg leading-relaxed font-medium">{dashboard.positionCommentary}</p>
                </div>
              </div>
            )}

            {dashboard.exogenousTrace && <ExogenousDecisionTracePanel trace={dashboard.exogenousTrace} />}

            <div className="grid grid-cols-1">
              <RecentDecisionsList decisions={dashboard.recentDecisions || []} />
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

        {dashboard && viewMode === "simulator" && (
          <main className="space-y-6 animate-fade-in">
            {marketStale && (
              <div className="bg-yellow-900/20 border border-yellow-500/40 text-yellow-100 px-4 py-3 rounded-lg">
                Market snapshot is older than 2 minutes. Consider refreshing market data before executing trades.
              </div>
            )}

            {manualError && (
              <div className="bg-red-900/20 border border-red-500/50 text-red-200 px-4 py-3 rounded-lg">
                {manualError}
              </div>
            )}

            {simulatorLoading && (
              <div className="text-gray-400 text-sm">Loading simulator data...</div>
            )}

            {manualPortfolio && (
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2 space-y-6">
                  <div>
                    <h2 className="text-gray-400 text-sm font-medium uppercase tracking-wider mb-3">Manual Portfolio</h2>
                    <PortfolioCard portfolio={manualPortfolio} />
                  </div>
                  <ManualTradeTicket
                    market={dashboard.market}
                    portfolio={manualPortfolio}
                    onExecute={handleManualTrade}
                    disabled={simulatorLoading}
                    error={manualError}
                  />
                  <TradeLogTable trades={combinedTrades} />
                </div>
                <div className="lg:col-span-1 space-y-6">
                  <MarketCard market={dashboard.market} />
                  <PerformanceComparisonPanel data={comparison} />
                  {dashboard.exogenousTrace && <ExogenousDecisionTracePanel trace={dashboard.exogenousTrace} />}
                </div>
              </div>
            )}
          </main>
        )}
      </div>
    </div>
  );
}

export default App;
