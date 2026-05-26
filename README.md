# Student Management SaaS

Multi-tenant student management platform — ASP.NET Core 9 modular monolith, Angular 19 admin, Flutter parent app, MySQL.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) — MySQL, Redis, RabbitMQ

## Local infrastructure (Docker)

```bash
# From repo root
copy docker\.env.example docker\.env
docker compose -f docker/docker-compose.yml up -d
docker compose -f docker/docker-compose.yml ps
```

| Service | Port | UI / notes |
|---------|------|------------|
| MySQL 8 | 3306 | DB `schoolsaas_dev`, root password from `docker/.env` |
| Redis 7 | 6379 | `redis-cli ping` → PONG |
| RabbitMQ | 5672, 15672 | Management UI http://localhost:15672 (guest/guest) |
| API (optional) | 8080 | Built from `docker/Dockerfile.api` when using full compose stack |

Stop stack:

```bash
docker compose -f docker/docker-compose.yml down
```

## App configuration (all environments)

Redis is configured in:

| File | Purpose |
|------|---------|
| `appsettings.json` | Production defaults |
| `appsettings.Development.json` | Local dev (Docker hostnames) |
| `appsettings.Development.local.json` | Your secrets (gitignored) |

```json
"ConnectionStrings": {
  "Redis": "localhost:6379,abortConnect=false"
},
"Redis": {
  "ConnectionString": "localhost:6379,abortConnect=false",
  "InstancePrefix": "schoolsaas:dev"
}
```

Cache keys are tenant-scoped: `{InstancePrefix}:tenant:{tenantId}:{yourKey}`.

## Local secrets

```bash
copy src\SchoolSaaS.Api\appsettings.Development.local.json.example src\SchoolSaaS.Api\appsettings.Development.local.json
```

Set MySQL password to match `MYSQL_ROOT_PASSWORD` in `docker/.env`.

## Quick start

```bash
docker compose -f docker/docker-compose.yml up -d

dotnet ef database update --project src/SchoolSaaS.Infrastructure --startup-project src/SchoolSaaS.Api

dotnet build
dotnet run --project src/SchoolSaaS.Api

# Health (default Kestrel port 5275; Docker API uses 8080)
curl http://localhost:5275/health
curl http://localhost:5275/health/ready   # includes Redis PING
```

Build API image:

```bash
docker build -f docker/Dockerfile.api -t schoolsaas-api .
```

## Tests

```bash
dotnet test tests/SchoolSaaS.UnitTests/SchoolSaaS.UnitTests.csproj
dotnet test tests/SchoolSaaS.ArchitectureTests/SchoolSaaS.ArchitectureTests.csproj
dotnet test tests/SchoolSaaS.IntegrationTests/SchoolSaaS.IntegrationTests.csproj
```

CI runs on every PR via [`.github/workflows/ci-api.yml`](.github/workflows/ci-api.yml). Enable branch protection on `master` to require the **CI — API** check before merge.

## Documentation

- [Agent guide](AGENTS.md)
- [Sprint 1 subtasks](backlog/sprint-1-l5-subtasks.md)
- [MVP sprint guide](backlog/MVP-PHASE1-SPRINT-GUIDE.md)
