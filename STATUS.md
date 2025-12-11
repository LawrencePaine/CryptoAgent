# Spec compliance status

## Overall completion
Estimated completion: **85%**. SQLite persistence, repositories, exchange abstraction, and the background worker now align Phase 1 with the spec, though live exchange integration and some robustness gaps remain.

## Completed / present
- SQLite-backed `CryptoAgentDbContext` with Portfolio, Trade, PerformanceSnapshot, and MarketSnapshot tables plus initial migration and EF Core DI wiring for the API and EF tooling.【F:backend/backend/Data/CryptoAgentDbContext.cs†L1-L114】【F:backend/backend/Migrations/20241008000000_InitialCreate.cs†L1-L107】【F:backend/backend/Program.cs†L14-L68】
- Repositories replace JSON stores for portfolio, trades, and performance snapshots, including trade logging, daily counts, and snapshot append/read helpers.【F:backend/backend/Repositories/PortfolioRepository.cs†L1-L123】【F:backend/backend/Repositories/PerformanceRepository.cs†L1-L64】
- Background `AgentWorker` records market snapshots on a schedule and can invoke the agent loop, while `PaperExchangeClient` executes paper trades with taker fees and balance updates via the database.【F:backend/backend/Services/AgentWorker.cs†L1-L71】【F:backend/backend/Services/PaperExchangeClient.cs†L1-L136】
- Configurable fee and app settings plus `AgentMode` drive DI wiring between paper and (stub) live exchange clients, keeping the manual run-once API intact.【F:backend/backend/Program.cs†L21-L120】【F:backend/backend/Services/IExchangeClient.cs†L1-L36】【F:backend/backend/Services/CryptoComExchangeClient.cs†L1-L32】

## Missing / incomplete
- `CryptoComExchangeClient` remains a Phase 2 stub—live balance/trade calls, authentication, and fee handling are unimplemented.【F:backend/backend/Services/CryptoComExchangeClient.cs†L1-L32】
- Market data fetching relies solely on CoinGecko without retries or resilience, and agent execution cadence is a simple counter in the worker; additional hardening and real exchange fail-safes are still needed for live mode.【F:backend/backend/Services/AgentWorker.cs†L16-L36】【F:backend/backend/Services.cs†L13-L108】
- The frontend still assumes a fixed API base URL instead of using the Vite dev proxy, which may break non-default port setups.【F:frontend/src/api.ts†L1-L24】
