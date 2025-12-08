# Spec compliance status

## Overall completion
Estimated completion: **70%**. Core models, endpoint scaffolding, and frontend panels align with the spec, but market data wiring and a few integration details remain incomplete or broken.

## Completed / present
- Portfolio, market, trading, and config models include vault/high-watermark fields and DTO conversion helpers, matching the spec’s conceptual shapes.【F:backend/backend/Models.cs†L5-L172】
- JSON-backed PortfolioStore and PerformanceStore exist with initialization, trade logging, and snapshot appending capabilities.【F:backend/backend/Services.cs†L9-L202】
- RiskEngine applies trade limits, allocation checks, and processes buy/sell decisions; AgentService orchestrates market lookup, LLM call, risk gating, profit skimming, and performance snapshotting.【F:backend/backend/Services.cs†L204-L511】
- Minimal API exposes dashboard, run-once, and monthly performance endpoints; CORS and Swagger are configured for local dev.【F:backend/backend/Program.cs†L6-L134】
- Frontend types mirror backend DTOs, monthly performance UI is implemented, and the dashboard triggers agent runs and refreshes performance data.【F:frontend/src/types.ts†L1-L63】【F:frontend/src/components/MonthlyPerformance.tsx†L1-L47】【F:frontend/src/App.tsx†L1-L159】

## Missing / incomplete
- MarketDataService contains malformed braces and uses a non-injected `HttpClientFactory` in `GetHistoricalDataAsync`, so CoinGecko integration and technical indicator inputs will not compile/run as-is.【F:backend/backend/Services.cs†L68-L177】
- OpenAI chat client is registered, but API key/model depend solely on configuration; no guard rails for missing values are present, and agent prompts lack explicit fallback handling for malformed JSON beyond a simple catch-all HOLD.【F:backend/backend/Program.cs†L37-L44】【F:backend/backend/Services.cs†L444-L464】
- Frontend API base URL is hardcoded to `http://localhost:5088` instead of relying on Vite proxy/env, which may break deployments outside that port configuration.【F:frontend/src/api.ts†L1-L24】
