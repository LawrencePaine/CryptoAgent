import type {
    DashboardResponse,
    ExogenousRefreshJobStatus,
    ManualTradeRequest,
    ManualTradeResponse,
    MonthlyPerformance,
    PerformanceCompareResponse,
    PortfolioDto,
    Trade,
} from "./types";

const API_BASE = "http://localhost:5088"; // Adjust if needed

export const api = {
    getDashboard: async (): Promise<DashboardResponse> => {
        const res = await fetch(`${API_BASE}/api/dashboard`);
        if (!res.ok) throw new Error("Failed to fetch dashboard");
        return res.json();
    },

    runAgent: async (): Promise<DashboardResponse> => {
        const res = await fetch(`${API_BASE}/api/agent/run-once`, {
            method: "POST",
        });
        if (!res.ok) throw new Error("Failed to run agent");
        return res.json();
    },

    getMonthlyPerformance: async (): Promise<MonthlyPerformance[]> => {
        const res = await fetch(`${API_BASE}/api/performance/monthly`);
        if (!res.ok) throw new Error("Failed to fetch performance");
        return res.json();
    },

    refreshExogenousContext: async (): Promise<{ jobId: string }> => {
        const res = await fetch(`${API_BASE}/api/exogenous/refresh`, { method: "POST" });
        if (!res.ok) {
            const payload = await res.json().catch(() => ({}));
            throw new Error(payload?.status || "Failed to refresh context");
        }
        return res.json();
    },

    getRefreshStatus: async (jobId: string): Promise<ExogenousRefreshJobStatus> => {
        const res = await fetch(`${API_BASE}/api/exogenous/refresh/${jobId}`);
        if (!res.ok) throw new Error("Failed to fetch refresh status");
        return res.json();
    },

    getManualPortfolio: async (): Promise<PortfolioDto> => {
        const res = await fetch(`${API_BASE}/api/manual-portfolio`);
        if (!res.ok) throw new Error("Failed to fetch manual portfolio");
        return res.json();
    },

    executeManualTrade: async (payload: ManualTradeRequest): Promise<ManualTradeResponse> => {
        const res = await fetch(`${API_BASE}/api/manual-trades`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload),
        });
        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || "Failed to execute trade");
        }
        return res.json();
    },

    getTrades: async (book: "AGENT" | "MANUAL", take = 20): Promise<Trade[]> => {
        const res = await fetch(`${API_BASE}/api/trades?book=${book}&take=${take}`);
        if (!res.ok) throw new Error("Failed to fetch trades");
        return res.json();
    },

    getPerformanceCompare: async (fromUtc?: string, toUtc?: string): Promise<PerformanceCompareResponse> => {
        const params = new URLSearchParams();
        if (fromUtc) params.append("from", fromUtc);
        if (toUtc) params.append("to", toUtc);
        const suffix = params.toString() ? `?${params.toString()}` : "";
        const res = await fetch(`${API_BASE}/api/performance/compare${suffix}`);
        if (!res.ok) throw new Error("Failed to fetch performance comparison");
        return res.json();
    },
};
