# Docker-based local development

This repository supports full local development with Docker Compose.

## Prerequisites

- Docker Desktop (or Docker Engine + Compose v2)

## Run the stack

```bash
docker compose up --build
```

## Stop the stack

```bash
docker compose down
```

## Reset local database and volumes

```bash
docker compose down -v
```

## Logs (API)

```bash
docker compose logs -f api
```

## Run one-off API commands

```bash
docker compose exec api <command>
```

Examples:

```bash
docker compose exec api dotnet ef database update
```

The API already applies EF migrations on startup, so manual migrations are usually unnecessary for local development.

## Typical dev URLs

- UI: http://localhost:5173
- API: http://localhost:8080
- Health: http://localhost:8080/health

## Environment configuration

1. Copy `.env.example` to `.env`.
2. Fill in provider/API keys as needed.

Compose loads `.env` automatically.

## Persistence

- SQLite database path inside API container: `/data/cryptoapp.db`
- Persisted by the named volume: `cryptoagent_data`
- Data survives `docker compose down` / `up`
- Data is wiped by `docker compose down -v`
