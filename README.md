# Student Management SaaS

Multi-tenant student management platform — ASP.NET Core 9 layered monolith, Angular 19 admin (planned), Flutter parent app (planned), **MySQL 8**.

## Current implementation (API)

| Area | Status |
|------|--------|
| Platform catalog DB (`schoolsaas_platform`) | `PlatformDbContext` — tenants, settings, permission catalog |
| Per-tenant DB (`ss_t_{slug}`) | `ApplicationDbContext` — users, RBAC, audit, outbox |
| Auth | `POST /api/v1/auth/login`, `/refresh`, `/logout`, `/register`, `/forgot-password`, `/reset-password`, `/accept-invitation` · `GET /me` · `PUT /me/password` |
| Users | `POST /api/v1/users/invite` |
| Audit | `GET /api/v1/audit-logs` |
| Tenant provisioning | `POST /api/v1/tenants` (requires `institution.tenant.create`) |
| Academic years | `GET/POST /api/v1/academic-years`, `POST …/{id}/set-current` (`institution.academic-years.manage`) |
| Grades / classes | `GET/POST /api/v1/grades`, `GET/POST /api/v1/classes`, `POST …/sections` (`institution.classes.manage`) |
| Staff | `GET/POST/PUT /api/v1/staff`, `POST …/{id}/link-user`, custom fields on create/update (`institution.staff.manage`) |
| Custom fields | `GET/POST/PUT /api/v1/custom-fields?entityType=Staff|Student` (`institution.staff.fields.manage`) |
| Documents | `GET/POST/DELETE /api/v1/staff/{id}/documents` (`institution.staff.documents.manage`); student routes ready for Sprint 4 (`students.documents.manage`) |
| Health | `GET /health`, `GET /health/ready`, `GET /api/v1/ping` |
| Super-admin user | **Not seeded** — `super_admin` JWT role bypasses RBAC when present |
| Password reset / invite / audit list | **Implemented** (emails logged via `LogEmailSender` in dev) |

## Solution layout

```text
src/
  SchoolSaaS.Api/Endpoints/          # one subfolder per module
    Auth/                            AuthEndpoints.cs       (Identity)
    Users/                           UserEndpoints.cs         (Identity)
    Tenants/                         TenantEndpoints.cs       (Institution)
    Institution/                     AcademicYearEndpoints.cs (Institution)
    Audit/                           AuditEndpoints.cs        (Audit)
    Platform/                        PlatformEndpoints.cs     (Platform)
    Rbac/                            RoleEndpoints.cs         (RBAC)
  SchoolSaaS.Application/
    Commands/{Area}/{Endpoint}/      # Command + Handler + Validator per endpoint
    Queries/{Area}/{Endpoint}/       # Query + Handler (+ Validator)
    Abstractions/                    # IRepository, IEmailSender, …
    Behaviors/                       # MediatR pipeline
  SchoolSaaS.Domain/
  SchoolSaaS.Infrastructure/
  SchoolSaaS.Shared/
```

Example: `POST /api/v1/auth/login` → `Application/Commands/Auth/Login/LoginCommand.cs`.

Future optional split: `src/Modules/{ModuleName}/` (not used yet).

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
| MySQL 8 | 3306 | Platform DB `schoolsaas_platform`; per-tenant DBs `ss_t_{slug}` (see below) |
| Redis 7 | 6379 | `redis-cli ping` → PONG |
| RabbitMQ | 5672, 15672 | Management UI http://localhost:15672 (guest/guest) |
| API (optional) | 8080 | Built from `docker/Dockerfile.api` when using full compose stack |

Stop stack:

```bash
docker compose -f docker/docker-compose.yml down
```

## Database architecture (per-tenant MySQL)

| Connection / database | Purpose |
|-----------------------|---------|
| **`ConnectionStrings:Platform`** (main) | Platform catalog on `schoolsaas_platform`: tenant registry, `db_server` / `db_user` / encrypted `db_password`, permission catalog |
| **`ss_t_{slug}`** on each tenant’s server | Dedicated MySQL host per institute — users, RBAC, audit, outbox, app data |

Each row in `tenants` stores where that institute’s database lives:

| Column | Meaning |
|--------|---------|
| `db_server` / `db_port` | Tenant’s MySQL host (can differ from the platform server) |
| `db_name` | Database name on that host |
| `db_user` | MySQL login for the app |
| `db_password` | **Encrypted at rest** (ASP.NET Data Protection); never store plaintext in the catalog |

Optional config:

- `ConnectionStrings:Provisioning` — admin login used to `CREATE DATABASE` on the **tenant’s server** (host/port taken from the tenant row; user/password from this connection string)
- `Tenancy:ProvisioningDbUser` / `ProvisioningDbPassword` — alternative to `Provisioning` when you do not use a full connection string

On first **Development** startup, the API migrates the platform DB, registers the `demo` tenant (with encrypted DB password), creates `ss_t_demo` on the configured server, migrates it, and seeds the admin user.

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

## Dev login

On **Development** startup the API migrates both contexts, seeds the `demo` tenant (`ss_t_demo`), syncs the permission catalog, and creates a bootstrap admin.

| Field | Value |
|-------|--------|
| Tenant slug | `demo` (subdomain or header `X-Tenant-Slug: demo`) |
| Email | `admin@demo.school` |
| Password | `Admin123!ChangeMe` |

Login uses **email + password** (not a separate username field).

```bash
curl -X POST http://localhost:5275/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Slug: demo" \
  -d "{\"email\":\"admin@demo.school\",\"password\":\"Admin123!ChangeMe\"}"
```

Use the returned `accessToken` as `Authorization: Bearer {token}` on protected routes.

### Create another tenant (tenant admin)

The demo `tenant_admin` role includes `institution.tenant.create` (not the same as platform super-admin):

```bash
curl -X POST http://localhost:5275/api/v1/tenants \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Slug: demo" \
  -H "Authorization: Bearer {accessToken}" \
  -d "{\"name\":\"Oakwood High\",\"slug\":\"oakwood\",\"plan\":\"free\",\"adminEmail\":\"admin@oakwood.school\",\"adminPassword\":\"Admin123!ChangeMe\"}"
```

Optional body fields: `dbServer`, `dbPort`, `dbName`, `dbUser`, `dbPassword`, `adminFirstName`, `adminLastName`. Defaults come from `Tenancy` in `appsettings.Development.json`.

### Super admin

There is **no seeded super-admin account**. Super-admin is recognized only when the JWT includes role claim `super_admin` (middleware sets `ITenantContext.IsSuperAdmin`). Platform super-admin login and cross-tenant APIs are backlog items.

## Quick start

```bash
docker compose -f docker/docker-compose.yml up -d

dotnet ef database update --context PlatformDbContext \
  --project src/SchoolSaaS.Infrastructure --startup-project src/SchoolSaaS.Api

# Tenant DBs (e.g. ss_t_demo) — use tenant connection or design-time factory;
# Development startup also runs tenant migrations via IdentityDataSeeder.

dotnet build
dotnet run --project src/SchoolSaaS.Api

# Health (default Kestrel port 5275; Docker API uses 8080)
curl http://localhost:5275/health
curl http://localhost:5275/health/ready   # includes Redis PING
curl http://localhost:5275/api/v1/ping  # requires auth
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

- [Agent guide](AGENTS.md) — architecture, implemented APIs, DB layout
- [Master module plan](student_management_saas_complete_module_plan.md) — “As implemented” section at top
- [ER diagrams](docs/module-er-diagrams.md) — logical models (MySQL, DB-per-tenant)
- [Logging policy](docs/universal-logging-policy.md)
- [Sprint 1 subtasks](backlog/sprint-1-l5-subtasks.md)
- [MVP sprint guide](backlog/MVP-PHASE1-SPRINT-GUIDE.md)
