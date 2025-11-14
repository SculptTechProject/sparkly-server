# Sparkly Server

Backend API for **Sparkly**, a build‑in‑public social platform. This repository contains the C# / .NET backend that powers authentication, user accounts and the core application features used by the Sparkly web client.

> Status: early development – API surface and architecture are evolving.

---

## Tech stack

* **.NET**: .NET 9 (ASP.NET Core Web API)
* **Data access**: Entity Framework Core (code‑first migrations)
* **Database**: PostgreSQL (via Docker) or local dev database
* **Containerization**: Docker + Docker Compose
* **Tooling**: `dotnet` CLI, EF Core CLI

---

## Project structure

High‑level layout of the repository:

```text
sparkly-server/
├─ Migrations/              # EF Core migrations
├─ src/                     # Application source code (API, domain, infrastructure)
├─ Program.cs               # Application bootstrap / entrypoint
├─ compose.yaml             # Docker Compose stack (API + database)
├─ Dockerfile               # Image for the API service
├─ appsettings.json         # Base configuration (non‑secret)
└─ appsettings.Development.json  # Local overrides (DO NOT commit secrets)
```

The `src` folder is where the actual application code lives (controllers, domain models, services, etc.). As the project grows, this will be organized into clear layers (e.g. `Api`, `Application`, `Domain`, `Infrastructure`).

---

## Getting started

### Prerequisites

* .NET 9 SDK installed
* Docker + Docker Compose installed (for running the full stack)
* Git

### 1. Clone the repository

```bash
git clone https://github.com/SculptTechProject/sparkly-server.git
cd sparkly-server
```

### 2. Configure environment

The backend expects configuration from `appsettings.json` and environment variables. **Secrets must never be committed to the repo.**

Create a local environment file (for your own use) or set environment variables via your shell / Docker:

```bash
# Example – adjust names/values to match the codebase
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=sparkly;Username=sparkly;Password=changeme"

# Example if you add auth / JWT later
export Jwt__Issuer="https://sparkly.local"
export Jwt__Audience="sparkly-app"
export Jwt__Secret="super-long-random-secret-key-change-me"
```

Keep a non‑secret example in the repo as `appsettings.Development.example.json` or `.env.example` (recommended), and use it to document required keys.

### 3. Apply database migrations (optional but recommended)

If EF Core migrations are used, apply them before running the API:

```bash
dotnet restore

dotnet ef database update
```

If you use Docker with a database container, you can also let the application apply migrations on startup (depending on how the bootstrapping is implemented).

### 4. Run the API locally

**Option A – `dotnet run`**

```bash
dotnet restore

dotnet run
```

By default the API will listen on the ports defined in `appsettings.json` / `launchSettings` / environment variables (commonly `http://localhost:5000` or `http://localhost:8080`).

**Option B – Docker Compose (API + DB)**

```bash
docker compose up --build
```

This will:

* build the backend image using `Dockerfile`,
* start the API container,
* start the database container defined in `compose.yaml`.

Check the logs to confirm that the API is healthy and connected to the database.

---

## API surface (high‑level)

This backend is responsible for the core Sparkly features, for example:

* user registration and login,
* user profile data and settings,
* Sparkly dashboard / feed backend endpoints,
* future billing / subscriptions integration (Stripe),
* admin / internal endpoints for moderation and analytics.

As the project grows, consider documenting endpoints using:

* **OpenAPI / Swagger** (Swashbuckle),
* or minimal API documentation in `README` (auth endpoints, example requests/responses).

---

## Docker

### Build image manually

```bash
docker build -t sparkly-server .
```

### Run container manually

```bash
docker run \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=db;Port=5432;Database=sparkly;Username=sparkly;Password=changeme" \
  -p 8080:8080 \
  sparkly-server
```

In practice you will usually prefer `docker compose up` because it brings up the database and the API together.

---

## Configuration & secrets

**Important:**

* API keys, JWT secrets, Stripe keys, and real database credentials **must never** live in the Git repository.
* Use environment variables / secret managers in development and production.

Recommended pattern:

* commit a `.env.example` (or `appsettings.Development.example.json`) with all required keys but without real secrets,
* document each key in a short comment / table so contributors know what to set.

Example keys you are likely to add as Sparkly evolves:

* `Stripe__SecretKey`
* `Stripe__WebhookSecret`
* `Jwt__Secret`
* `Jwt__Issuer`
* `Jwt__Audience`

---

## Development workflow

Suggested workflow while the project is young:

1. Create a small issue / task (feature, refactor, bugfix).
2. Work on a feature branch.
3. Add or update tests (unit/integration) around new behaviour.
4. Run tests and `dotnet build` locally.
5. Open a PR (even if you are the only contributor – PR history becomes project documentation).

This keeps the history clean and makes it easier to reason about changes later.

---

## Roadmap ideas

Some directions for the Sparkly backend:

* Authentication & authorization layer (JWT, refresh tokens, roles/permissions).
* First version of the "build in public" feed (posts, comments, reactions).
* Real‑time communication (SignalR or WebSockets) for live rooms / chats.
* Stripe integration for paid plans (billing, webhooks, subscription status synced to users).
* Observability (logging, metrics, health checks, readiness / liveness probes for Docker / Kubernetes).

---

## Contributing

This project is currently developed by the SculptTech / Sparkly team. External contributions are welcome once the core architecture stabilises.

If you want to propose a change:

* open an issue with a short description and motivation,
* or open a draft PR with your idea.

---

## License

License: **TBD**

Until a license is added, treat this repository as source‑available but not licensed for unrestricted commercial reuse.
