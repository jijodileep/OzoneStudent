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
| Backend modules | `src/Modules/{ModuleName}/` |
| Shared kernel | `src/SchoolSaaS.Shared/`, `src/SchoolSaaS.Domain/` |
| API host | `src/SchoolSaaS.Api/` |
| Angular admin | `frontend/src/app/` |
| Flutter parent app | `mobile/lib/` |
| Tests | `tests/` |

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
- [ ] Command/Query + Handler
- [ ] FluentValidation validator
- [ ] `[RequirePermission]` attribute
- [ ] Audit logging
- [ ] API endpoint at `/api/v1/...`
- [ ] Unit test
- [ ] Integration test (if DB involved)

## Current Phase

**MVP Phase 1** — See sprint guide for scope. Phase 2 adds Finance/LMS/Student App/Social.
