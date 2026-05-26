# Student Management SaaS

Multi-tenant student management platform — ASP.NET Core 9 modular monolith, Angular 19 admin, Flutter parent app, MySQL.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for local MySQL, Redis, RabbitMQ — Sprint 1 D9+)

## Local secrets

Copy the example file and set your MySQL password (this file is gitignored):

```bash
cp src/SchoolSaaS.Api/appsettings.Development.local.json.example src/SchoolSaaS.Api/appsettings.Development.local.json
```

## Quick start

```bash
# Restore and build
dotnet build

# Run API (Sprint 1 stub)
dotnet run --project src/SchoolSaaS.Api

# Run tests
dotnet test
```

## Local infrastructure (coming Sprint 1 D9)

```bash
docker compose -f docker/docker-compose.yml up -d
```

## Documentation

- [Agent guide](AGENTS.md)
- [Sprint 1 subtasks](backlog/sprint-1-l5-subtasks.md)
- [MVP sprint guide](backlog/MVP-PHASE1-SPRINT-GUIDE.md)
