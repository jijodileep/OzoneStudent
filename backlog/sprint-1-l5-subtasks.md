# Sprint 1 — L5 Technical Subtask Expansion (Critical Path)

**Sprint:** 1 (Weeks 1–2)  
**Goal:** Solution builds, tenant isolation proven, outbox works, CI green, Docker local stack running  
**Epic:** Platform Foundation + DevOps baseline  
**Total L4 tasks:** 30 | **Total L5 subtasks:** ~142  
**Estimated effort:** 70–80 dev-hours (1 backend + 0.5 DevOps)

---

## How to Use This Document

| Level | Name | Example |
|---|---|---|
| L4 | Small Engineering Task | `CreateTenantQueryFilter` |
| L5 | Technical Subtask | `Add ITenantEntity marker interface` |

**Workflow:** Pick one L4 task → complete all L5 subtasks in order → run Verification → mark done in Linear/Jira.

**Cursor prompt prefix for every L5 subtask:**

```markdown
Sprint 1 / Task: {L4TaskName} / Subtask: {L5SubtaskName}
Follow: .cursor/rules/saas-platform-core.mdc + relevant backend rules
Do NOT implement tasks outside this subtask scope.
```

---

## Execution Schedule (Recommended)

| Day | L4 Tasks | Owner |
|---|---|---|
| **D1** | CreateSolutionStructure, DefineResultPattern, CreateTestProjectStructure | Backend |
| **D2** | DefineBaseEntity, ConfigureEfCorePostgreSql, CreateTenantsTableMigration | Backend |
| **D3** | DefineITenantContext, ImplementTenantContextMiddleware, CreateTenantQueryFilter | Backend |
| **D4** | CreateTenantSaveChangesInterceptor, RegisterMediatRPipeline, CreateValidationBehavior | Backend |
| **D5** | CreateLoggingBehavior, CreateTenantBehavior, DefineGlobalExceptionHandler | Backend |
| **D6** | ConfigureApiVersioning, ConfigureSwaggerOpenApi, ConfigureFluentValidationAssemblyScan | Backend |
| **D7** | DefineIntegrationEventBase → CreateOutboxPublisherBackgroundService | Backend |
| **D8** | ConfigureRedisConnection, CreateICacheService, CreateArchitectureTestRules | Backend |
| **D9** | TenantIsolationIntegrationTest, CreateDockerComposeLocal | Backend + DevOps |
| **D10** | CreateDockerfileApi, CiBuildApiWorkflow, sprint review | DevOps |

---

# L4 TASK 01: CreateSolutionStructure

**Points:** 3 | **Blocked by:** none | **Blocks:** almost everything

## L5 Subtasks

- [ ] **1.1** Run `dotnet new sln -n SchoolSaaS` at repo root
- [ ] **1.2** Create projects:
  - `src/SchoolSaaS.Api` (ASP.NET Core Web, net9.0)
  - `src/SchoolSaaS.Application` (class lib)
  - `src/SchoolSaaS.Domain` (class lib)
  - `src/SchoolSaaS.Infrastructure` (class lib)
  - `src/SchoolSaaS.Shared` (class lib)
- [ ] **1.3** Add all projects to solution; set references:
  - Application → Domain, Shared
  - Infrastructure → Application, Domain
  - Api → Application, Infrastructure
  - Domain → Shared only
- [ ] **1.4** Create folder scaffold:
  ```
  src/Modules/.gitkeep
  src/Modules/Platform/Domain/
  src/Modules/Platform/Application/
  src/Modules/Platform/Infrastructure/
  src/Modules/Platform/Api/
  ```
- [ ] **1.5** Add NuGet packages to Api: `MediatR`, `FluentValidation`, `Serilog.AspNetCore`, `Swashbuckle.AspNetCore`
- [ ] **1.6** Create `Program.cs` minimal host stub returning `"School SaaS API"`
- [ ] **1.7** Add `Directory.Build.props` with `TreatWarningsAsErrors=true`, `Nullable=enable`
- [ ] **1.8** Add root `.gitignore` (.NET, Docker, IDE, `.env`)
- [ ] **1.9** Add root `README.md` with local run instructions placeholder
- [ ] **1.10** Verify: `dotnet build` succeeds with zero warnings

## Files Created

| Path | Purpose |
|---|---|
| `SchoolSaaS.sln` | Solution root |
| `Directory.Build.props` | Shared MSBuild properties |
| `src/SchoolSaaS.*/` | Layer projects |

## Verification

```bash
dotnet build
dotnet run --project src/SchoolSaaS.Api
# Expect: app starts on https://localhost:5xxx
```

---

# L4 TASK 02: DefineResultPattern

**Points:** 2 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **2.1** Create `src/SchoolSaaS.Shared/Results/Error.cs` — record with `Code`, `Message`, `Field?`
- [ ] **2.2** Create `ValidationError.cs` extending Error with `PropertyName`
- [ ] **2.3** Create `Result.cs` — non-generic with `IsSuccess`, `Errors[]`, static `Success()` / `Failure()`
- [ ] **2.4** Create `Result<T>.cs` — generic with `Value`, implicit conversion guards
- [ ] **2.5** Add factory methods: `Result.NotFound()`, `Result.Forbidden()`, `Result.Conflict()`
- [ ] **2.6** Unit test: `Result_Success_HasNoErrors`, `Result_Failure_ContainsErrorCode`

## Files Created

`src/SchoolSaaS.Shared/Results/{Error,ValidationError,Result,ResultT}.cs`

## Verification

```bash
dotnet test --filter "Result"
```

---

# L4 TASK 03: DefineBaseEntity

**Points:** 2 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **3.1** Create `ITenantEntity` marker interface with `Guid TenantId { get; set; }`
- [ ] **3.2** Create `ISoftDeletable` with `bool IsDeleted { get; set; }`
- [ ] **3.3** Create `BaseEntity` abstract class: `Id`, `TenantId`, `CreatedAt`, `CreatedBy?`, `UpdatedAt?`, `UpdatedBy?`, `IsDeleted`
- [ ] **3.4** Create `AuditableEntity` extending BaseEntity — add `LastModifiedReason?` (optional)
- [ ] **3.5** Create `IAggregateRoot` marker interface
- [ ] **3.6** Create `IDomainEvent` interface with `DateTime OccurredAt`
- [ ] **3.7** Add `BaseEntity` constructor initializing `Id = Guid.NewGuid()`, `CreatedAt = UtcNow`

## Files Created

`src/SchoolSaaS.Domain/Common/{ITenantEntity,ISoftDeletable,BaseEntity,AuditableEntity,IAggregateRoot,IDomainEvent}.cs`

## Verification

Architecture test (added later) confirms Domain has no EF references.

---

# L4 TASK 04: ConfigureEfCorePostgreSql

**Points:** 3 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **4.1** Add NuGet: `Npgsql.EntityFrameworkCore.PostgreSQL`, `EFCore.NamingConventions` to Infrastructure
- [ ] **4.2** Create `ApplicationDbContext.cs` in Infrastructure/Persistence
- [ ] **4.3** Configure snake_case: `.UseSnakeCaseNamingConvention()` in options
- [ ] **4.4** Create `IUnitOfWork` in Domain + `UnitOfWork` implementation wrapping DbContext
- [ ] **4.5** Create `DependencyInjection.cs` extension: `AddInfrastructure(services, configuration)`
- [ ] **4.6** Read connection string from `ConnectionStrings:DefaultConnection`
- [ ] **4.7** Add `appsettings.Development.json` with local PostgreSQL connection string
- [ ] **4.8** Register DbContext as scoped in DI
- [ ] **4.9** Add EF Core design-time factory for migrations CLI
- [ ] **4.10** Wire Infrastructure DI from `Program.cs`

## Files Created

| Path | Purpose |
|---|---|
| `Infrastructure/Persistence/ApplicationDbContext.cs` | DbContext |
| `Infrastructure/Persistence/UnitOfWork.cs` | Transaction boundary |
| `Infrastructure/DependencyInjection.cs` | DI registration |

## Verification

```bash
dotnet ef migrations add InitialBaseline --project src/SchoolSaaS.Infrastructure --startup-project src/SchoolSaaS.Api
dotnet ef database update
```

---

# L4 TASK 05: CreateTenantsTableMigration

**Points:** 2 | **Blocked by:** ConfigureEfCorePostgreSql, DefineBaseEntity

## L5 Subtasks

- [ ] **5.1** Create `Tenant` entity in `Modules/Platform/Domain/Entities/Tenant.cs`
  - Fields: `Name`, `Slug`, `Status` (enum), `Plan`, `SettingsJson`, inherits BaseEntity (TenantId = self Id for tenant table OR use non-tenant base — **tenant table exempt from tenant filter**)
- [ ] **5.2** Create `TenantStatus` enum: `Active`, `Suspended`, `Pending`
- [ ] **5.3** Create `TenantConfiguration.cs` — unique index on `slug`
- [ ] **5.4** Register `DbSet<Tenant>` in ApplicationDbContext
- [ ] **5.5** Generate migration: `20260101_Platform_CreateTenantsTable`
- [ ] **5.6** Review generated SQL for snake_case columns and indexes

## Important Design Decision

`Tenant` entity is **platform-level** — do NOT apply tenant query filter to this entity. Document with `[ExcludeFromTenantFilter]` attribute or separate DbContext section.

## Verification

Migration applies; `\d tenants` in psql shows correct schema.

---

# L4 TASK 06: CreateTenantSettingsTableMigration

**Points:** 1 | **Blocked by:** CreateTenantsTableMigration

## L5 Subtasks

- [ ] **6.1** Create `TenantSetting` entity: `TenantId`, `Key`, `Value`, unique `(tenant_id, key)`
- [ ] **6.2** Create EF configuration with FK to tenants
- [ ] **6.3** Generate migration
- [ ] **6.4** Seed no data (runtime seed in Sprint 3)

## Verification

FK constraint exists; unique composite index present.

---

# L4 TASK 07: DefineITenantContext

**Points:** 2 | **Blocked by:** DefineBaseEntity

## L5 Subtasks

- [ ] **7.1** Create `ITenantContext` interface:
  ```csharp
  Guid? TenantId { get; }
  Guid? BranchId { get; }
  Guid? UserId { get; }
  bool IsAuthenticated { get; }
  bool IsSuperAdmin { get; }
  ```
- [ ] **7.2** Create `TenantContext` scoped implementation (mutable set by middleware)
- [ ] **7.3** Create `ITenantContextAccessor` if needed for interceptors
- [ ] **7.4** Register as scoped in DI
- [ ] **7.5** Unit test: default context has null TenantId

## Files Created

`src/SchoolSaaS.Shared/MultiTenancy/{ITenantContext,TenantContext}.cs`

---

# L4 TASK 08: ImplementTenantContextMiddleware

**Points:** 3 | **Blocked by:** DefineITenantContext

## L5 Subtasks

- [ ] **8.1** Create `TenantContextMiddleware.cs` in Api/Middleware
- [ ] **8.2** Resolve tenant from JWT claim `tenant_id` (parse Guid)
- [ ] **8.3** Fallback: read `X-Tenant-Id` header for dev/swagger only (config flag)
- [ ] **8.4** Populate `TenantContext` from claims: `sub` → UserId
- [ ] **8.5** Skip tenant resolution for paths: `/health`, `/swagger`, `/api/v1/auth/login`
- [ ] **8.6** Return 401 if authenticated request has missing tenant (except super-admin)
- [ ] **8.7** Register middleware after `UseAuthentication()`, before `UseAuthorization()`
- [ ] **8.8** Integration test stub: middleware sets context correctly from test JWT

## Verification

Request with valid JWT claim populates `ITenantContext.TenantId`.

---

# L4 TASK 09: CreateTenantQueryFilter

**Points:** 3 | **Blocked by:** ConfigureEfCorePostgreSql, DefineITenantContext

## L5 Subtasks

- [ ] **9.1** Create `[ExcludeFromTenantFilter]` attribute for platform entities (Tenant)
- [ ] **9.2** In `ApplicationDbContext.OnModelCreating`, loop entity types implementing `ITenantEntity`
- [ ] **9.3** Apply filter: `e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted`
- [ ] **9.4** Inject `ITenantContext` via `ITenantContextAccessor` (singleton-safe pattern for DbContext)
- [ ] **9.5** Create test entity `TestTenantEntity` in integration test project only
- [ ] **9.6** Verify filter generates SQL with `WHERE tenant_id = @p`

## Critical Pattern

Use `ITenantContextAccessor` — NOT direct scoped `ITenantContext` injection into DbContext constructor (lifetime issue).

## Verification

Log SQL via `EnableSensitiveDataLogging` in dev; confirm tenant_id in WHERE clause.

---

# L4 TASK 10: CreateTenantSaveChangesInterceptor

**Points:** 2 | **Blocked by:** CreateTenantQueryFilter

## L5 Subtasks

- [ ] **10.1** Create `TenantSaveChangesInterceptor` extending `SaveChangesInterceptor`
- [ ] **10.2** On `Added` entities implementing `ITenantEntity`: set `TenantId` from context
- [ ] **10.3** Throw `CrossTenantWriteException` if entity TenantId != context TenantId
- [ ] **10.4** On `Added`/`Modified`: stamp `CreatedAt`/`UpdatedAt`, `CreatedBy`/`UpdatedBy`
- [ ] **10.5** Register interceptor in DbContext options
- [ ] **10.6** Unit test: added entity gets TenantId stamped

## Verification

Insert without explicit TenantId → saved with correct tenant_id in DB.

---

# L4 TASK 11: RegisterMediatRPipeline

**Points:** 2 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **11.1** Add `MediatR` to Application project
- [ ] **11.2** Create `DependencyInjection.cs` in Application: `AddApplication()`
- [ ] **11.3** Register MediatR scanning Application + all Modules/*/Application assemblies
- [ ] **11.4** Create `ICommand<T>` marker extending `IRequest<Result<T>>`
- [ ] **11.5** Create `IQuery<T>` marker extending `IRequest<Result<T>>`
- [ ] **11.6** Wire `AddApplication()` from Api Program.cs
- [ ] **11.7** Smoke test: create `PingQuery` returning `"pong"` + test endpoint

## Verification

`IMediator.Send(new PingQuery())` returns success from test endpoint.

---

# L4 TASK 12: CreateValidationBehavior

**Points:** 2 | **Blocked by:** RegisterMediatRPipeline

## L5 Subtasks

- [ ] **12.1** Add `FluentValidation.DependencyInjectionExtensions`
- [ ] **12.2** Create `ValidationBehavior<TRequest,TResponse>` implementing `IPipelineBehavior`
- [ ] **12.3** Inject `IEnumerable<IValidator<TRequest>>`
- [ ] **12.4** Run validators; on failure return `Result.Failure(validationErrors)` without calling next
- [ ] **12.5** Register behavior in MediatR pipeline (order: first)
- [ ] **12.6** Test with intentional invalid command → ValidationError returned

## Verification

Handler NOT called when validation fails (mock handler or counter).

---

# L4 TASK 13: CreateLoggingBehavior

**Points:** 2 | **Blocked by:** RegisterMediatRPipeline

## L5 Subtasks

- [ ] **13.1** Configure Serilog in Program.cs: JSON console sink, enrich from log context
- [ ] **13.2** Create `LoggingBehavior<TRequest,TResponse>` — log Request name, elapsed ms
- [ ] **13.3** Do NOT log request body (PII risk) — log type name only
- [ ] **13.4** Log warnings on Result failure with error codes
- [ ] **13.5** Register after ValidationBehavior

## Verification

Console shows structured log: `{RequestName} completed in {ElapsedMs}ms`.

---

# L4 TASK 14: CreateTenantBehavior

**Points:** 2 | **Blocked by:** DefineITenantContext, RegisterMediatRPipeline

## L5 Subtasks

- [ ] **14.1** Create `TenantBehavior<TRequest,TResponse>` pipeline behavior
- [ ] **14.2** For requests implementing `ITenantScopedRequest` marker: verify TenantId not null
- [ ] **14.3** Return `Result.Forbidden()` if tenant context missing on scoped request
- [ ] **14.4** Register in pipeline after ValidationBehavior

## Verification

Command marked tenant-scoped fails gracefully without tenant context.

---

# L4 TASK 15: DefineGlobalExceptionHandler

**Points:** 3 | **Blocked by:** DefineResultPattern

## L5 Subtasks

- [ ] **15.1** Create `GlobalExceptionHandler.cs` implementing `IExceptionHandler`
- [ ] **15.2** Map `CrossTenantWriteException` → 403 ProblemDetails
- [ ] **15.3** Map `EntityNotFoundException` → 404
- [ ] **15.4** Map unhandled → 500 (hide stack in production)
- [ ] **15.5** Include `correlationId` in ProblemDetails.Extensions
- [ ] **15.6** Register via `AddExceptionHandler<GlobalExceptionHandler>()`
- [ ] **15.7** Create `CorrelationIdMiddleware` — read/generate `X-Correlation-Id`

## Verification

Throw test exception → RFC 7807 JSON response with correct status code.

---

# L4 TASK 16: ConfigureApiVersioning

**Points:** 2 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **16.1** Add `Asp.Versioning.Http` + `Asp.Versioning.Mvc.ApiExplorer`
- [ ] **16.2** Configure default version 1.0, assume default when unspecified
- [ ] **16.3** Version via URL segment: `/api/v{version}/...`
- [ ] **16.4** Create `Api/V1/EndpointRouteBuilderExtensions.cs` for endpoint registration
- [ ] **16.5** Add sample versioned health endpoint: `GET /api/v1/health`

## Verification

`/api/v1/health` returns 200; `/api/v2/health` returns 404.

---

# L4 TASK 17: ConfigureSwaggerOpenApi

**Points:** 2 | **Blocked by:** ConfigureApiVersioning

## L5 Subtasks

- [ ] **17.1** Configure SwaggerGen with JWT bearer security scheme
- [ ] **17.2** Add `X-Tenant-Id` header parameter documentation (dev only)
- [ ] **17.3** Enable Swagger UI in Development environment only
- [ ] **17.4** Group endpoints by module tags
- [ ] **17.5** Include XML comments if present

## Verification

Swagger UI loads; Authorize button accepts Bearer token.

---

# L4 TASK 18: ConfigureFluentValidationAssemblyScan

**Points:** 1 | **Blocked by:** CreateValidationBehavior

## L5 Subtasks

- [ ] **18.1** In `AddApplication()`: `services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>()`
- [ ] **18.2** Scan all module Application assemblies via marker classes
- [ ] **18.3** Verify validator auto-resolved in ValidationBehavior

## Verification

Add test validator; DI resolves it without manual registration.

---

# L4 TASK 19: DefineIntegrationEventBase

**Points:** 2 | **Blocked by:** DefineBaseEntity

## L5 Subtasks

- [ ] **19.1** Create `IntegrationEventBase` record: `EventId`, `TenantId`, `OccurredAt`, `EventType` (string)
- [ ] **19.2** Create `IIntegrationEventHandler<TEvent>` interface
- [ ] **19.3** Create `IEventPublisher` abstraction (implemented by outbox in L4-21)
- [ ] **19.4** Add `EventType` auto-set from class name in constructor

## Files Created

`src/SchoolSaaS.Shared/Events/{IntegrationEventBase,IIntegrationEventHandler,IEventPublisher}.cs`

---

# L4 TASK 20: CreateOutboxMessageEntity

**Points:** 2 | **Blocked by:** DefineIntegrationEventBase

## L5 Subtasks

- [ ] **20.1** Create `OutboxMessage` entity: `Id`, `EventId`, `EventType`, `PayloadJson`, `OccurredAt`, `ProcessedAt?`, `Error?`, `RetryCount`
- [ ] **20.2** Create EF configuration; index on `(processed_at IS NULL, occurred_at)`
- [ ] **20.3** Generate migration `CreateOutboxMessagesTable`
- [ ] **20.4** Create `ProcessedEvent` entity for consumer idempotency: `EventId`, `ProcessedAt`

## Verification

Migration creates `outbox_messages` and `processed_events` tables.

---

# L4 TASK 21: ImplementOutboxSaveChangesInterceptor

**Points:** 3 | **Blocked by:** CreateOutboxMessageEntity

## L5 Subtasks

- [ ] **21.1** Create `AggregateRoot` base adding `_domainEvents` list + `AddDomainEvent()`
- [ ] **21.2** Create `OutboxSaveChangesInterceptor` on SaveChanges
- [ ] **21.3** Collect events from changed aggregate roots
- [ ] **21.4** Serialize to JSON → insert `OutboxMessage` rows in same transaction
- [ ] **21.5** Clear domain events after capture
- [ ] **21.6** Register interceptor on DbContext

## Verification

Save entity with domain event → outbox row inserted atomically; rollback removes outbox row.

---

# L4 TASK 22: CreateOutboxPublisherBackgroundService

**Points:** 5 | **Blocked by:** ImplementOutboxSaveChangesInterceptor

## L5 Subtasks

- [ ] **22.1** Add NuGet `RabbitMQ.Client`
- [ ] **22.2** Create `RabbitMqOptions` config section: Host, Port, Username, Password, Exchange
- [ ] **22.3** Create `RabbitMqConnectionFactory` wrapper with retry
- [ ] **22.4** Create `OutboxPublisherBackgroundService` : `BackgroundService`
- [ ] **22.5** Poll unprocessed outbox messages (batch 50, every 5s)
- [ ] **22.6** Publish to RabbitMQ exchange with routing key = event type
- [ ] **22.7** On success: set `ProcessedAt`
- [ ] **22.8** On failure: increment `RetryCount`, store error, dead-letter after 5 retries
- [ ] **22.9** Create `TestEventPublisherIntegrationTest` with Testcontainers RabbitMQ
- [ ] **22.10** Register hosted service in DI

## Verification

Insert outbox row manually → background service publishes → ProcessedAt set within 10s.

---

# L4 TASK 23: ConfigureRedisConnection

**Points:** 2 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **23.1** Add `StackExchange.Redis` + `Microsoft.Extensions.Caching.StackExchangeRedis`
- [ ] **23.2** Create `RedisOptions` config: ConnectionString, InstancePrefix
- [ ] **23.3** Register `IConnectionMultiplexer` as singleton
- [ ] **23.4** Register `IDistributedCache` with Redis backend
- [ ] **23.5** Add Redis to docker-compose with healthcheck

## Verification

App starts and connects to Redis; `PING` returns `PONG`.

---

# L4 TASK 24: CreateICacheService

**Points:** 2 | **Blocked by:** ConfigureRedisConnection

## L5 Subtasks

- [ ] **24.1** Create `ICacheService`: `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, `RemoveByPrefixAsync`
- [ ] **24.2** Implement `RedisCacheService` with key format: `{prefix}:tenant:{tenantId}:{key}`
- [ ] **24.3** Register as scoped (uses ITenantContext for prefix)
- [ ] **24.4** Unit test: tenant A key != tenant B key for same logical key

## Verification

Set cache for tenant A; tenant B cannot read it.

---

# L4 TASK 25: CreateTestProjectStructure

**Points:** 2 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **25.1** Create `tests/SchoolSaaS.UnitTests` (xUnit, FluentAssertions, NSubstitute)
- [ ] **25.2** Create `tests/SchoolSaaS.IntegrationTests` (xUnit, Testcontainers.PostgreSql, WebApplicationFactory)
- [ ] **25.3** Create `tests/SchoolSaaS.ArchitectureTests` (NetArchTest.Rules)
- [ ] **25.4** Add all test projects to solution
- [ ] **25.5** Add CI-friendly `runsettings` for code coverage (optional)
- [ ] **25.6** Create `IntegrationTestBase` with WebApplicationFactory + PostgreSQL container

## Verification

```bash
dotnet test
# All projects compile; 0 tests or placeholder passes
```

---

# L4 TASK 26: CreateArchitectureTestRules

**Points:** 2 | **Blocked by:** CreateTestProjectStructure

## L5 Subtasks

- [ ] **26.1** Test: Domain does not reference Infrastructure, Application, Api
- [ ] **26.2** Test: Application does not reference Infrastructure, Api
- [ ] **26.3** Test: Handlers reside in Application or Modules/*/Application
- [ ] **26.4** Test: All classes ending in `Handler` implement `IRequestHandler<,>`
- [ ] **26.5** Test: All entities inherit BaseEntity or are explicitly exempt (Tenant)

## Verification

```bash
dotnet test tests/SchoolSaaS.ArchitectureTests
```

---

# L4 TASK 27: TenantIsolationIntegrationTest

**Points:** 3 | **Blocked by:** CreateTenantQueryFilter, CreateTestProjectStructure

## L5 Subtasks

- [ ] **27.1** Create `TestProduct` entity (tenant-scoped) in IntegrationTests/Fixtures only OR Platform module
- [ ] **27.2** Seed tenant A + tenant B with products in each
- [ ] **27.3** Test: context as tenant A → query returns only A products
- [ ] **27.4** Test: tenant A cannot update tenant B product by ID (404 or exception)
- [ ] **27.5** Test: insert with wrong TenantId throws CrossTenantWriteException
- [ ] **27.6** Add test to CI required checks

## Verification

Test fails if query filter removed — confirms test validity.

---

# L4 TASK 28: CreateDockerComposeLocal

**Points:** 3 | **Blocked by:** none (parallel)

## L5 Subtasks

- [ ] **28.1** Create `docker/docker-compose.yml`:
  - `postgres:16` — port 5432, volume, healthcheck
  - `redis:7` — port 6379
  - `rabbitmq:3-management` — ports 5672, 15672
- [ ] **28.2** Create `docker/.env.example` with default credentials
- [ ] **28.3** Create `docker/init-db.sql` placeholder (optional extensions: uuid-ossp)
- [ ] **28.4** Document in README: `docker compose -f docker/docker-compose.yml up -d`
- [ ] **28.5** Verify all services healthy within 30s

## Verification

```bash
docker compose -f docker/docker-compose.yml ps
# All services: healthy
```

---

# L4 TASK 29: CreateDockerfileApi

**Points:** 2 | **Blocked by:** CreateSolutionStructure

## L5 Subtasks

- [ ] **29.1** Create multi-stage `docker/Dockerfile.api`:
  - Stage 1: `mcr.microsoft.com/dotnet/sdk:9.0` — restore, build, publish
  - Stage 2: `mcr.microsoft.com/dotnet/aspnet:9.0` — copy publish output
- [ ] **29.2** Run as non-root user `appuser`
- [ ] **29.3** Expose port 8080; set `ASPNETCORE_URLS=http://+:8080`
- [ ] **29.4** Add HEALTHCHECK calling `/health`
- [ ] **29.5** Add API service to docker-compose referencing Dockerfile

## Verification

```bash
docker build -f docker/Dockerfile.api -t schoolsaas-api .
docker run -p 8080:8080 schoolsaas-api
curl http://localhost:8080/health
```

---

# L4 TASK 30: CiBuildApiWorkflow

**Points:** 3 | **Blocked by:** CreateTestProjectStructure, CreateDockerfileApi

## L5 Subtasks

- [ ] **30.1** Create `.github/workflows/ci-api.yml`
- [ ] **30.2** Trigger: pull_request to `main` and `develop`
- [ ] **30.3** Jobs: `build-and-test`
  - Setup .NET 9
  - Start PostgreSQL + Redis + RabbitMQ services
  - `dotnet restore`, `dotnet build --no-restore`
  - `dotnet test --no-build --verbosity normal`
- [ ] **30.4** Fail PR if any test fails or build has warnings (TreatWarningsAsErrors)
- [ ] **30.5** Cache NuGet packages
- [ ] **30.6** Upload test results artifact
- [ ] **30.7** Add branch protection rule requiring CI pass (document in README)

## Verification

Push branch → GitHub Actions runs green.

---

# Sprint 1 Exit Checklist

| # | Criterion | Verified |
|---|---|---|
| 1 | `dotnet build` zero warnings | [ ] |
| 2 | `dotnet test` all pass including architecture + tenant isolation | [ ] |
| 3 | `docker compose up` starts postgres, redis, rabbitmq | [ ] |
| 4 | API Docker image builds and `/health` responds | [ ] |
| 5 | CI pipeline green on PR | [ ] |
| 6 | Tenant query filter enforced (integration test) | [ ] |
| 7 | Outbox message published to RabbitMQ (integration test) | [ ] |
| 8 | Swagger accessible with JWT scheme documented | [ ] |
| 9 | MediatR pipeline: Validation → Logging → Tenant behaviors work | [ ] |
| 10 | All code follows `.cursor/rules/` conventions | [ ] |

---

# Sprint 1 → Sprint 2 Handoff

When all exit criteria pass, these are **ready for Sprint 2**:

- `CreateUserEntity` can inherit `BaseEntity`
- Auth middleware can populate `ITenantContext.UserId`
- `CreateAuthorizationBehavior` plugs into existing MediatR pipeline
- `AuditBehavior` plugs into existing MediatR pipeline
- Outbox ready for `UserRegisteredIntegrationEvent`

**Do NOT start Sprint 2 until** `TenantIsolationIntegrationTest` is in CI and passing.

---

# Quick Reference: L4 → L5 Count

| L4 Task | L5 Count |
|---|---|
| CreateSolutionStructure | 10 |
| DefineResultPattern | 6 |
| DefineBaseEntity | 7 |
| ConfigureEfCorePostgreSql | 10 |
| CreateTenantsTableMigration | 6 |
| CreateTenantSettingsTableMigration | 4 |
| DefineITenantContext | 5 |
| ImplementTenantContextMiddleware | 8 |
| CreateTenantQueryFilter | 6 |
| CreateTenantSaveChangesInterceptor | 6 |
| RegisterMediatRPipeline | 7 |
| CreateValidationBehavior | 6 |
| CreateLoggingBehavior | 5 |
| CreateTenantBehavior | 4 |
| DefineGlobalExceptionHandler | 7 |
| ConfigureApiVersioning | 5 |
| ConfigureSwaggerOpenApi | 5 |
| ConfigureFluentValidationAssemblyScan | 3 |
| DefineIntegrationEventBase | 4 |
| CreateOutboxMessageEntity | 4 |
| ImplementOutboxSaveChangesInterceptor | 6 |
| CreateOutboxPublisherBackgroundService | 10 |
| ConfigureRedisConnection | 5 |
| CreateICacheService | 4 |
| CreateTestProjectStructure | 6 |
| CreateArchitectureTestRules | 5 |
| TenantIsolationIntegrationTest | 6 |
| CreateDockerComposeLocal | 5 |
| CreateDockerfileApi | 5 |
| CiBuildApiWorkflow | 7 |
| **TOTAL** | **~167** |
