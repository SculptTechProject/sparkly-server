# Sparkly Server

Backend API for **Sparkly**, a build‑in‑public social platform. This repository contains the C# / .NET backend that powers authentication, user accounts, project management and the core application features used by the Sparkly web client.

> Status: early development – API surface, architecture and testing pipeline are actively evolving.

---

## Tech stack

* **.NET**: .NET 9 (ASP.NET Core Web API)
* **Data access**: Entity Framework Core (code‑first migrations)
* **Database**: PostgreSQL (local or via Docker)
* **Containerization**: Docker + Docker Compose
* **Tests**: xUnit integration tests using TestServer
* **CI/CD**: GitHub Actions (build + tests + optional Docker build)
* **Tooling**: `dotnet` CLI, EF Core CLI

---

## Project structure

```text
sparkly-server/
├─ src/                     # API, domain, infrastructure
├─ Migrations/              # EF Core migrations
├─ sparkly-server.test/     # Integration tests
├─ Program.cs               # Entrypoint
├─ compose.yaml             # Docker Compose stack (API + DB)
├─ Dockerfile               # API image
├─ .github/workflows/       # GitHub Actions CI
├─ appsettings.json         # Base config (non-secret)
└─ appsettings.Development.json  # Local overrides
```

The `src` directory contains controllers, domain models, services, repositories and configuration. Tests live in a separate project with isolated database state.

---

## Getting started

### Prerequisites

* .NET 9 SDK
* Docker + Docker Compose
* Git

### Clone

```bash
git clone https://github.com/SculptTechProject/sparkly-server.git
cd sparkly-server
```

### Environment configuration

The backend reads configuration from `appsettings.json` and environment variables. Do not commit secrets.

Example variables:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=sparkly;Username=sparkly;Password=changeme"
export Jwt__Issuer="https://sparkly.local"
export Jwt__Audience="sparkly-app"
export Jwt__Secret="super-long-random-secret-key-change-me"
```

A `.env.example` or `appsettings.Development.example.json` is recommended to document required keys.

### Database migrations

```bash
dotnet restore
dotnet ef database update
```

Or let Docker apply migrations at startup, depending on configuration.

### Run locally

**Dotnet CLI:**

```bash
dotnet restore
dotnet run
```

**Docker Compose:**

```bash
docker compose up --build
```

This builds the API image and launches both the API and PostgreSQL.

---

## Tests

The project includes integration tests that run the API using an in-memory test server. Tests reset database state for every run.

Run tests locally:

```bash
dotnet test
```

Run tests using Docker Compose:

```bash
docker compose run --rm api dotnet test
```

---

## GitHub Actions (CI)

This repository contains a CI pipeline that runs on every push and pull request:

* restore and build
* run tests
* optionally build Docker image

This ensures that the API and tests stay green across contributions.

---

## API overview

The backend currently covers:

* user registration and login
* user profile and authentication
* project creation and management

Planned additions:

* feed system for build-in-public updates
* real‑time notifications
* billing and subscription logic
* moderation and admin endpoints

Documentation will be available through OpenAPI/Swagger.

---

## Docker commands

Build image manually:

```bash
docker build -t sparkly-server .
```

Run image manually:

```bash
docker run \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=db;Port=5432;Database=sparkly;Username=sparkly;Password=changeme" \
  -p 8080:8080 \
  sparkly-server
```

---

## Secrets

Secrets must always be supplied using environment variables or secret management tools. Never commit real credentials.

Likely future keys:

* Stripe secrets
* JWT settings
* OAuth providers (GitHub, Google)

---

## Development workflow

1. Create a small issue or task.
2. Implement changes on a feature branch.
3. Add or update tests.
4. Run local build and tests.
5. Push and open a PR.

This keeps the project clean and easy to maintain.

---

## Roadmap (short‑term)

* Full authentication and refresh tokens
* Public project pages
* Build-in-public feed
* Email notifications
* Admin panel foundations
* Observability (structured logs, metrics, probes)

---

## Contributing

The project is actively developed by the SculptTech / Sparkly team. External contributions will be welcomed once core systems stabilise.

---

## License

License: **TBD**

Until then, treat the repository as source‑available only.
