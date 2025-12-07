import type { DashboardResponse, MonthlyPerformance } from "./types";

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
};
