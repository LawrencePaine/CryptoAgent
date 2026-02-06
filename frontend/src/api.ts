import type {
    DashboardResponse,
    ExogenousRefreshResponse,
    ManualTradeRequest,
    ManualTradeResponse,
    MonthlyPerformance,
    PerformanceCompareResponse,
    PortfolioDto,
    Trade,
} from "./types";

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080";

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

    refreshExogenousContext: async (): Promise<ExogenousRefreshResponse> => {
        const res = await fetch(`${API_BASE}/api/exogenous/refresh`, { method: "POST" });
        if (res.status === 409) {
            return res.json();
        }
        if (!res.ok) {
            const payload = await res.json().catch(() => ({}));
            throw new Error(payload?.message || "Failed to refresh context");
        }
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
