# MVP Phase 1 — Sprint-Ready Backlog Guide

## Implementation alignment (May 2026)

Backlog tasks describe **target** scope. The repo today:

| Area | Status |
|------|--------|
| Stack | ASP.NET Core 9, EF Core, **MySQL**, Redis, RabbitMQ, Serilog |
| Tenancy | `PlatformDbContext` + per-tenant `ApplicationDbContext` (`ss_t_{slug}`) |
| Sprint 1 core | MediatR pipeline, tenant filter/interceptor, outbox, Docker, CI — largely done |
| Sprint 2 auth | login, refresh, logout, register, me, forgot/reset/change password, invite, accept-invitation, audit list — done |
| Application layout | `Commands/{Area}/{Endpoint}/`, `Queries/{Area}/{Endpoint}/` (see `AGENTS.md`) |
| Sprint 3 tenant | `POST /api/v1/tenants`, `CreateTenantCommand`, `ITenantOnboardingService` — done; academic structure — not done |
| Super admin | Not seeded; use demo `tenant_admin` + `institution.tenant.create` for tenant creation |

Paths in [`sprint-1-l5-subtasks.md`](sprint-1-l5-subtasks.md) that reference `src/Modules/` or PostgreSQL are **planning targets** — implement under `src/SchoolSaaS.*`, MySQL, and **`Application/Commands|Queries/{Area}/{Endpoint}/`** unless a task explicitly migrates layout.

---

## Files Included

| File | Use For |
|---|---|
| `mvp-phase1-linear-import.csv` | **Linear** — Import via Settings → Workspace → Import |
| `mvp-phase1-jira-import.csv` | **Jira** — Import via External System Import (CSV) |
| `sprint-1-l5-subtasks.md` | **Sprint 1 deep dive** — 167 L5 subtasks with file paths, verification steps |
| `sprint-1-l5-linear-import.csv` | **Linear** — Sprint 1 L5 subtasks as child issues (sample critical tasks) |
| This guide | Sprint planning, team assignment, Cursor workflow |
| [`mvp-phase1-all-sprints.md`](mvp-phase1-all-sprints.md) | **All sprints** — master doc with every L4 task, exit criteria, handoffs |

---

## Sprint Calendar (12 Weeks / 6 Sprints)

| Sprint | Weeks | Theme | Story Points (est.) | Exit Criteria |
|---|---|---|---|---|
| **Sprint 1** | 1–2 | Platform Foundation + DevOps baseline | ~55 | Solution builds, tenant isolation proven, CI green, Docker local stack |
| **Sprint 2** | 3–4 | Identity, RBAC, Audit, Angular auth | ~60 | Login works, permissions enforced, audit captured, admin can login |
| **Sprint 3** | 5–6 | Institution + Admin shell | ~65 | Tenant provisioning, academic structure CRUD, admin navigation |
| **Sprint 4** | 7–8 | Student Management | ~55 | Student CRUD, guardians, enrollment, admin UI complete |
| **Sprint 5** | 9–10 | Attendance + Fees (structure/invoicing) | ~60 | Daily attendance marking, fee structures, invoice generation |
| **Sprint 6** | 11–12 | Payments, Notifications, Parent App, Deploy | ~75 | End-to-end MVP live on staging VPS |

**Total estimated story points:** ~370 (team velocity dependent)

---

## How to Import

### Linear

1. Open **Settings → Workspace → Import/Export → Import CSV**
2. Upload `mvp-phase1-linear-import.csv`
3. Map columns:
   - `Title` → Title
   - `Description` → Description
   - `Priority` → Priority (1 = Urgent, 4 = Low)
   - `Estimate` → Estimate (story points)
   - `Labels` → Labels (semicolon-separated)
   - `Project` → Project (create "Student SaaS" first)
   - `Milestone` → Milestone ("MVP Phase 1")
   - `Parent issue` → Parent (for epic hierarchy)
   - `Blocked by` → Blocked by (semicolon-separated titles)
4. After import, create **Cycles** named Sprint 1–6 and assign issues by `Project` column value

### Jira

1. **Settings → System → External System Import → CSV**
2. Upload `mvp-phase1-jira-import.csv`
3. Map fields:
   - `Issue Type` → Issue Type
   - `Summary` → Summary
   - `Description` → Description
   - `Priority` → Priority
   - `Story Points` → Story Points (custom field)
   - `Epic Name` → Epic Link
   - `Sprint` → Sprint (create sprints first, or assign post-import)
   - `Labels` → Labels
   - `Component` → Component
   - `Acceptance Criteria` → Description append or custom field
   - `Depends On` → Issue Links (blocks/is blocked by)
4. Create **Components**: Backend, Frontend, Mobile, Database, DevOps, Testing
5. Create **Sprints** Sprint 1 through Sprint 6 in your board

---

## Team Assignment Matrix

| Stream | Owner | Sprints Active | Primary Epics |
|---|---|---|---|
| **Backend Core** | Backend Dev 1 | 1–6 | Platform Foundation, Identity, Audit |
| **Backend Domain** | Backend Dev 2 | 3–6 | Institution, Student, Attendance, Fees |
| **Frontend Admin** | Frontend Dev | 2–6 | Admin Web App |
| **Mobile Parent** | Mobile Dev | 6 | Parent Mobile App |
| **DevOps** | DevOps / Full-stack | 1, 6 | DevOps & Deployment |
| **QA** | QA / Any dev | 2–6 | Testing tasks across epics |

---

## Sprint 1 — Detailed Task List (Critical Path)

These tasks **must** complete before Sprint 2 starts:

```
CreateSolutionStructure
DefineBaseEntity
ConfigureEfCoreMySql
DefineITenantContext
ImplementTenantContextMiddleware
CreateTenantQueryFilter
CreateTenantSaveChangesInterceptor
RegisterMediatRPipeline
CreateValidationBehavior
CreateLoggingBehavior
CreateTenantBehavior
DefineResultPattern
DefineGlobalExceptionHandler
ConfigureApiVersioning
ConfigureSwaggerOpenApi
CreateTenantsTableMigration
DefineIntegrationEventBase
CreateOutboxMessageEntity → ImplementOutboxSaveChangesInterceptor → CreateOutboxPublisherBackgroundService
ConfigureRedisConnection → CreateICacheService
TenantIsolationIntegrationTest
CreateDockerComposeLocal
CreateDockerfileApi
CiBuildApiWorkflow
```

---

## Sprint 2 — Detailed Task List

```
CreateUserEntity + CreateUsersTableMigration
RegisterUserCommand → InviteUserCommand → AcceptInvitationCommand
LoginCommand → GenerateJwtTokenService → RefreshTokenCommand → RevokeRefreshTokenCommand
LoginAttemptTracker
ForgotPasswordCommand → ResetPasswordCommand → ChangePasswordCommand
All auth endpoints (Login, Refresh, Logout, Register, Invite, Me)
DefinePermissionCatalog → SeedPermissionsCommand
CreateRoleEntity → CreateRoleCommand → AssignPermissionsToRoleCommand → AssignRoleToUserCommand
PermissionResolverService → CreateAuthorizationBehavior → ScopeResolverService
SeedDefaultRolesMigration
All RBAC endpoints
CreateAuditLogEntity → AuditLogService → CreateAuditBehavior → EntityChangeCaptureInterceptor
ListAuditLogsEndpoint
CreateAngularWorkspace → AuthService → LoginPageComponent → AuthGuard → TokenRefreshInterceptor
```

---

## Definition of Done (All Tasks)

Every task is **done** when:

- [ ] Code merged to `develop` branch
- [ ] Unit test(s) pass for handler/service logic
- [ ] Integration test passes where applicable
- [ ] RBAC permission defined and enforced (if API/command)
- [ ] Audit entry written on write operations
- [ ] Tenant isolation verified (no cross-tenant leak)
- [ ] FluentValidation validator exists (if command)
- [ ] Swagger documents endpoint (if API)
- [ ] No linter/architecture test violations

---

## Cursor Prompt Template (Copy Per Task)

```markdown
## Task: {TaskName}
**Sprint:** {SprintN} | **Epic:** {EpicName} | **Points:** {N}

### Context
- Project: Multi-tenant Student Management SaaS
- Stack: ASP.NET Core 9, Clean Architecture, CQRS/MediatR, EF Core, MySQL (platform + per-tenant DBs)
- Module: {ModuleName}

### Dependencies (already implemented)
- {List completed dependency tasks}

### Requirements
1. Multi-tenant: use ITenantContext, global query filter, auto-stamp TenantId
2. RBAC: [RequirePermission("{module}.{resource}.{action}")]
3. Audit: call IAuditService in command handler
4. Validation: FluentValidation validator class
5. Soft delete: set IsDeleted = true, never hard delete
6. API: REST endpoint at /api/v1/{resource}
7. Result pattern: return Result<T>, never throw for business errors
8. Tests: unit test for handler + integration test if DB involved

### Acceptance Criteria
{From CSV Acceptance Criteria column}

### Deliverables
- [ ] Domain entity (if new)
- [ ] EF configuration + migration (if new table)
- [ ] Command/Query + Handler
- [ ] Validator
- [ ] Endpoint (Minimal API)
- [ ] Unit test
- [ ] Integration test (if applicable)
```

---

## Risk Register (MVP)

| Risk | Sprint | Mitigation |
|---|---|---|
| Tenant data leakage | 1 | TenantIsolationIntegrationTest in CI gate |
| RBAC bypass | 2 | AuthorizationBehavior + architecture test for [RequirePermission] |
| Outbox event loss | 1 | Outbox pattern with idempotent consumer |
| Sprint 6 scope creep | 6 | Parent app read-only; defer payment gateway to Phase 2 |
| Frontend/backend drift | 3+ | Generate Angular services from OpenAPI spec weekly |

---

## Out of Scope (Phase 2)

- Finance & double-entry accounting auto-posting
- LMS (courses, assignments, quizzes)
- Student mobile app
- Social networking
- Online payment gateway integration
- Bulk student import
- Advanced reporting dashboards
- SMS/push notifications (email + in-app only in MVP)

---

## Post-Import Checklist

- [ ] Create project/workspace "Student SaaS"
- [ ] Create milestone "MVP Phase 1"
- [ ] Create 6 sprints/cycles
- [ ] Import CSV
- [ ] Verify epic → story hierarchy
- [ ] Set up "Blocked by" links for dependency chains
- [ ] Assign team members to streams
- [ ] Configure Cursor rules referencing this backlog path
- [ ] Schedule Sprint 1 planning meeting with critical path tasks
