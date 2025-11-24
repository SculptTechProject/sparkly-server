# Sparkly Server

Backend API for **Sparkly**, a platform for documenting progress, building projects consistently and creating a dynamic portfolio. This repository contains the C# / .NET backend that powers authentication, user accounts, project timelines and the core application logic used by the Sparkly web client.

> Status: early development – the API, domain model and testing setup continue to evolve.

---

## Tech stack

* **.NET 9** (ASP.NET Core Web API)
* **Entity Framework Core** (code-first migrations)
* **PostgreSQL** (local or Docker)
* **Docker + Docker Compose** for containerization
* **xUnit** integration tests with TestServer
* **GitHub Actions** (build, tests, optional Docker build)
* `dotnet` CLI and EF Core CLI

---

## Project structure

```text
sparkly-server/
├─ src/                     # API, domain and infrastructure
├─ Migrations/              # EF Core migrations
├─ sparkly-server.test/     # Integration tests
├─ Program.cs               # Application entrypoint
├─ compose.yaml             # Docker Compose stack
├─ Dockerfile               # API image
├─ .github/workflows/       # CI pipelines
├─ appsettings.json         # Base config
└─ appsettings.Development.json  # Local overrides
```

`src` contains controllers, services, domain entities, validators and repository logic. Tests run in isolation with a clean database state.

---

## Getting started

### Requirements

* .NET 9 SDK
* Docker + Docker Compose
* Git

### Clone the repo

```bash
git clone https://github.com/SculptTechProject/sparkly-server.git
cd sparkly-server
```

### Environment variables

Configuration comes from `appsettings.json` and environment variables. Secrets should never be committed.

Example setup:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=sparkly;Username=sparkly;Password=changeme"
export Jwt__Issuer="https://sparkly.local"
export Jwt__Audience="sparkly-app"
export Jwt__Secret="super-long-secret-key-change-me"
```

Providing an example config file (`.env.example` or `appsettings.Development.example.json`) is recommended.

### Apply migrations

```bash
dotnet restore
dotnet ef database update
```

Docker can also run migrations automatically depending on the setup.

### Run locally

**Using dotnet:**

```bash
dotnet restore
dotnet run
```

**Using Docker Compose:**

```bash
docker compose up --build
```

This launches the API and PostgreSQL.

---

## Tests

Integration tests run the API with an isolated in-memory server. Each test resets database state.

Run locally:

```bash
dotnet test
```

Run through Docker Compose:

```bash
docker compose run --rm api dotnet test
```

---

## GitHub Actions CI

The repository includes a pipeline triggered on push and pull requests. It performs:

* restore and build
* test execution
* optional Docker image build

This ensures the API stays stable across changes.

---

## API overview

Current features:

* user authentication and login
* user profiles
* project creation and management

Planned development:

* project timelines and weekly logs
* build-in-public feed
* notifications
* subscriptions and billing
* admin and moderation tools

OpenAPI / Swagger documentation will be added.

---

## Docker usage

Build the image manually:

```bash
docker build -t sparkly-server .
```

Run the container:

```bash
docker run \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=db;Port=5432;Database=sparkly;Username=sparkly;Password=changeme" \
  -p 8080:8080 \
  sparkly-server
```

---

## Secrets

Secrets must be provided via environment variables or a secret manager.

Future keys may include:

* Stripe
* JWT
* OAuth providers (GitHub, Google)

---

## Development workflow

1. Open an issue or define a small task.
2. Implement the change in a feature branch.
3. Update or add tests.
4. Run the build and tests locally.
5. Open a pull request.

This keeps the repository clean and maintainable.

---

## Short-term roadmap

* refresh tokens
* public project pages
* project feed and logs
* email notifications
* admin foundations
* observability (logs, metrics, probes)

---

## Contributing

The codebase is currently maintained by the SculptTech / Sparkly team. External contributions will be accepted once the core platform stabilises.

---

## License

License: **TBD**

Until finalised, treat the code as source-available only.
