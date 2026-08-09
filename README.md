# TripConnect

TripConnect is a full-stack travel planning app with a .NET backend, Angular frontend, SQL Server, and Redis. The easiest way to run the whole project is with Docker Compose from the repository root.

## Requirements

- Docker Desktop installed and running
- Docker Compose available in your terminal

## Setup

1. Copy `.env.example` to `.env` in the repository root.
2. Set `MSSQL_SA_PASSWORD` and `JWT_SECRET_KEY` in `.env`.
3. Run `docker compose up --build` from the repository root.

That single command starts:

- SQL Server on `localhost:1433`
- Redis on `localhost:6379`
- API on `localhost:5126`
- Frontend on `localhost:4200`

## Open The App

- Frontend: `http://localhost:4200`
- API: `http://localhost:5126`
- Health check: `http://localhost:5126/api/health/check`

The frontend is served through nginx and proxies `/api` requests to the backend container, so browser calls to `http://localhost:4200/api/...` work without extra configuration.

## Useful Commands

Start in detached mode:

```powershell
docker compose up --build -d
```

Stop everything:

```powershell
docker compose down
```

Remove volumes as well:

```powershell
docker compose down -v
```

## Troubleshooting

- If Docker warns that `MSSQL_SA_PASSWORD` or `JWT_SECRET_KEY` is missing, check your root `.env` file.
- If the frontend shows an old page, hard refresh the browser.
- If the API is unavailable right after startup, wait a few seconds and try again while SQL Server finishes initializing.
