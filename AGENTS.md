# Student Management SaaS — Agent Guide

Multi-tenant Student Management SaaS. Modular monolith, ASP.NET Core 9, Angular 19, Flutter, MySQL.

**Master plan (all modules):** [`student_management_saas_complete_module_plan.md`](student_management_saas_complete_module_plan.md)  
**Logging policy (everything logged):** [`docs/universal-logging-policy.md`](docs/universal-logging-policy.md)  
**ER diagrams (all modules):** [`docs/module-er-diagrams.md`](docs/module-er-diagrams.md)

## Before Writing Code

1. Check backlog task in [`backlog/mvp-phase1-linear-import.csv`](backlog/mvp-phase1-linear-import.csv)
2. Sprint plan (all sprints): [`backlog/mvp-phase1-all-sprints.md`](backlog/mvp-phase1-all-sprints.md)
3. For Sprint 1 tasks, expand L5 subtasks in [`backlog/sprint-1-l5-subtasks.md`](backlog/sprint-1-l5-subtasks.md)
4. Cursor rules in [`.cursor/rules/`](.cursor/rules/) auto-apply by file glob — follow them strictly
5. Read [`backlog/MVP-PHASE1-SPRINT-GUIDE.md`](backlog/MVP-PHASE1-SPRINT-GUIDE.md) for sprint context and DoD

## Architecture Summary

| Layer | Location |
|---|---|
| API host | `src/SchoolSaaS.Api/` |
| Application (CQRS) | `src/SchoolSaaS.Application/` — `Commands/{Area}/{Endpoint}/`, `Queries/{Area}/{Endpoint}/` |
| Domain entities | `src/SchoolSaaS.Domain/` |
| Infrastructure (EF, JWT, outbox) | `src/SchoolSaaS.Infrastructure/` |
| Shared (results, tenancy, permissions) | `src/SchoolSaaS.Shared/` |
| Angular admin | `frontend/src/app/` (scaffold; not wired to all APIs yet) |
| Flutter parent app | `mobile/lib/` (scaffold) |
| Tests | `tests/` |

**As implemented:** layered monolith under `src/SchoolSaaS.*`. Future modular split may move code to `src/Modules/{ModuleName}/` per the master plan — do not assume that folder exists yet.

### CQRS folder layout (Application layer)

One folder per **endpoint operation**. Namespace = folder path.

```text
SchoolSaaS.Application/
  Commands/
    Auth/Login/              LoginCommand, LoginCommandHandler, LoginCommandValidator
    Auth/Logout/
    Auth/RefreshToken/
    Auth/Register/
    Auth/ForgotPassword/
    Auth/ResetPassword/
    Auth/ChangePassword/
    Auth/AcceptInvitation/
    Users/InviteUser/
    Tenants/CreateTenant/
  Queries/
    Auth/GetCurrentUser/
    Audit/ListAuditLogs/
    Platform/Ping/
  Abstractions/              Ports (IUserRepository, IAuditService, …)
  Behaviors/                 Validation, Authorization, Audit, Tenant, Logging
  Common/                    ICommand<T>, IQuery<T>, IPlatformCommand
```

| API route group | Commands / queries path |
|-----------------|-------------------------|
| `/api/v1/auth/*` | `Commands/Auth/{Operation}/` or `Queries/Auth/GetCurrentUser/` |
| `/api/v1/users/*` | `Commands/Users/InviteUser/` |
| `/api/v1/tenants` | `Commands/Tenants/CreateTenant/` |
| `/api/v1/audit-logs` | `Queries/Audit/ListAuditLogs/` |
| `/api/v1/ping` | `Queries/Platform/Ping/` |

**Namespace examples:** `SchoolSaaS.Application.Commands.Auth.Login`, `SchoolSaaS.Application.Queries.Audit.ListAuditLogs`.

Do **not** use legacy paths `Identity/Commands`, `Platform/Commands`, or flat `Application/Auth/`.

### Databases (MySQL)

| Context | Database | Contents |
|---------|----------|----------|
| `PlatformDbContext` | `schoolsaas_platform` | `tenants`, `tenant_settings`, global `permissions` catalog |
| `ApplicationDbContext` | `ss_t_{slug}` per tenant | users, RBAC, `audit_logs`, outbox, app data |

Never accept `TenantId` from the client body — resolve via JWT / `X-Tenant-Slug` / subdomain (`ITenantContext`). Platform commands use `IPlatformCommand` and `PlatformDbContext`.

### Implemented API surface (MVP in progress)

| Method | Path | Notes |
|--------|------|--------|
| POST | `/api/v1/auth/login` | Requires tenant (`X-Tenant-Slug` or subdomain) |
| POST | `/api/v1/auth/refresh` | |
| POST | `/api/v1/auth/logout` | |
| POST | `/api/v1/auth/register` | |
| POST | `/api/v1/auth/forgot-password` | Anonymous; always returns generic message |
| POST | `/api/v1/auth/reset-password` | Anonymous; requires `X-Tenant-Slug` |
| POST | `/api/v1/auth/accept-invitation` | Anonymous; requires `X-Tenant-Slug` |
| GET | `/api/v1/auth/me` | |
| PUT | `/api/v1/auth/me/password` | `auth.password.change` |
| POST | `/api/v1/users/invite` | `users.invite` |
| GET | `/api/v1/audit-logs` | `audit.logs.read` (paged) |
| POST | `/api/v1/tenants` | `institution.tenant.create` |
| GET | `/api/v1/ping` | Authenticated smoke test |
| GET | `/health`, `/health/ready` | No auth |

**Not implemented yet:** super-admin seed, suspend tenant, real email delivery (uses `LogEmailSender`).

## Non-Negotiables

- Tenant isolation via `ITenantContext` — never trust client `TenantId`
- RBAC on every command/query: `[RequirePermission("module.resource.action")]`
- Audit on every write operation
- FluentValidation on every command
- Soft delete only
- `Result<T>` pattern in handlers

## Rule Files Reference

| Rule | Applies To |
|---|---|
| `saas-platform-core.mdc` | Always |
| `backend-clean-architecture.mdc` | All C# |
| `backend-cqrs-patterns.mdc` | Application layer |
| `backend-multi-tenancy.mdc` | All C# |
| `backend-rbac.mdc` | All C# |
| `backend-audit.mdc` | All C# |
| `backend-api-endpoints.mdc` | Api layer |
| `backend-ef-core-database.mdc` | Domain + Infrastructure |
| `backend-event-driven.mdc` | Events/handlers |
| `module-identity-auth.mdc` | Identity module |
| `module-rbac-permissions.mdc` | RBAC module |
| `module-institution.mdc` | Institution module |
| `module-students.mdc` | Students module |
| `module-attendance.mdc` | Attendance module |
| `module-fees.mdc` | Fees module |
| `module-notifications.mdc` | Notifications module |
| `module-audit-logging.mdc` | Audit module |
| `frontend-angular-admin.mdc` | Angular frontend |
| `mobile-flutter-parent.mdc` | Flutter mobile |
| `devops-deployment.mdc` | Docker/CI/CD |
| `testing-standards.mdc` | Test projects |

## Task Implementation Checklist

When implementing a backlog task, deliver:

- [ ] Domain entity (if new)
- [ ] EF configuration + migration
- [ ] `Commands/{Area}/{Endpoint}/` or `Queries/{Area}/{Endpoint}/` (command/query + handler + validator)
- [ ] `[RequirePermission]` attribute
- [ ] Audit logging
- [ ] API endpoint at `/api/v1/...`
- [ ] Unit test
- [ ] Integration test (if DB involved)

## Current Phase

**MVP Phase 1** — See sprint guide for scope. Phase 2 adds Finance/LMS/Student App/Social.
