# SmartMaint — Smart Corporate Facility Maintenance Management Platform

Capstone project FA26SE147. CMMS for the management board of rental office buildings and apartment buildings.

| Layer | Tech | Folder |
|---|---|---|
| Backend API | .NET 10, ASP.NET Core Web API, EF Core 10 + Npgsql | [`backend/`](backend) |
| Web | React 19 + Vite + TypeScript, Tailwind CSS | [`frontend/`](frontend) |
| Mobile | Flutter (not scaffolded yet) | `mobile/` |
| Database | PostgreSQL 17 (local: Docker · deploy: Supabase) | [`database/`](database) |
| Deploy | Railway (API) + Vercel (web) | — |

**Rules for structure, naming, modules and pages: [`docs/architecture/CONVENTIONS.md`](docs/architecture/CONVENTIONS.md). Read it before writing code.**

## Repository layout

```
SmartMaint/
├── backend/             .NET solution: one API project split by folder (Controllers / Modules / Entities / Data / Common) + UnitTests
├── frontend/            React + Vite web app
├── database/            Schema.sql (generated from migrations) + Data.sql (seed)
├── scripts/             Helper scripts (migrations, schema export)
├── docs/                Requirements, flows F1–F6, plans, ERD, architecture conventions
├── docker-compose.yml   Local Postgres
└── .env.example         Backend + docker env variables (copy to .env)
```

## Prerequisites

- .NET SDK 10 (`dotnet --list-sdks` shows 10.0.x)
- Node.js 24+
- Docker Desktop

## First run

```bash
cp .env.example .env
```

```bash
docker compose up -d
```

```bash
dotnet run --project backend/src/SmartMaint.Api --launch-profile http
```

On first start the API applies migrations and loads `database/Data.sql`.
API: http://localhost:5080 · Swagger: http://localhost:5080/swagger · Health: http://localhost:5080/health

In a second terminal:

```bash
npm --prefix frontend install
```

```bash
npm --prefix frontend run dev
```

Web: http://localhost:5173 (Vite proxies `/api` to the API, so no CORS setup is needed locally).

Demo accounts are listed at the top of [`database/Data.sql`](database/Data.sql).

## Everyday commands

| Task | Command (from repo root) |
|---|---|
| Backend tests | `dotnet test backend` |
| Only Boundary tests | `dotnet test backend --filter "Type=Boundary"` |
| Add a migration + refresh Schema.sql | `pwsh scripts/db-add-migration.ps1 AddBuildings` |
| Refresh Schema.sql only | `pwsh scripts/db-schema.ps1` |
| Reset local DB | `docker compose down -v` then `docker compose up -d` and restart the API |
| Web typecheck / lint / format | `npm --prefix frontend run typecheck` · `run lint` · `run format` |
| Web production build | `npm --prefix frontend run build` |

## Deploy (summary)

- **API → Railway:** Dockerfile `backend/Dockerfile`, build context = repo root. Variables: same keys as `.env.example`
  (`ConnectionStrings__Default` = Supabase connection string, `Jwt__SigningKey`, `Cors__AllowedOrigins__0` = Vercel URL).
  Apply `database/Schema.sql` + `database/Data.sql` to Supabase via the SQL editor; the API never migrates on Railway.
- **Web → Vercel:** root directory `frontend`, framework Vite, env `VITE_API_BASE_URL` = Railway URL.
  `frontend/vercel.json` rewrites all routes to `index.html`.
