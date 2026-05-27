# Sprint 3 — L5 Technical Subtask Expansion (Critical Path)

> **Implementation note (May 2026):** Use **MySQL 8**, **`src/SchoolSaaS.*`**, CQRS folders **`Application/Commands/{Area}/{Endpoint}/`**, **`Application/Queries/{Area}/{Endpoint}/`**, and module-wise API folders **`Endpoints/{Module}/`**.

**Sprint:** 3 (Weeks 5–6)  
**Goal:** Tenant provisioning, academic structure CRUD, admin navigation shell  
**Epics:** Institution Management, Admin Web App  
**Total L4 tasks:** 39  
**Estimated effort:** 80–90 dev-hours (1 backend + 1 frontend)

---

## How to Use This Document

| Level | Name | Example |
|---|---|---|
| L4 | Small Engineering Task | `CreateAcademicYearCommand` |
| L5 | Technical Subtask | `Add IAcademicYearRepository` |

**Cursor prompt prefix:**

```markdown
Sprint 3 / Task: {L4TaskName} / Subtask: {L5SubtaskName}
Follow: .cursor/rules/saas-platform-core.mdc + module-institution.mdc
Do NOT implement tasks outside this subtask scope.
```

---

## Execution Schedule (Recommended)

| Day | L4 Tasks | Owner |
|---|---|---|
| **D1** | CreateAcademicYearEntity → CreateAcademicYearEndpoint | Backend |
| **D2** | CreateGradeEntity → CreateSectionCommand | Backend |
| **D3** | CreateSubjectEntity → CreateStaffCommand | Backend |
| **D4** | TenantProvisioningIntegrationTest, GetCurrentTenantEndpoint | Backend |
| **D5–D6** | AppShellLayoutComponent → PermissionGuard | Frontend |
| **D7–D8** | AcademicYearListComponent → GradeClassTreeComponent | Frontend |
| **D9** | RoleListPageComponent → PermissionMatrixComponent | Frontend |
| **D10** | Sprint review, exit checklist | All |

---

## Completed (Sprint 3 start)

- [x] `CreateTenantCommand`, `ITenantOnboardingService`, `POST /api/v1/tenants`
- [x] `AcademicYear`, `Term` entities + migration
- [x] Academic year commands/queries/endpoints
- [x] `Grade`, `SchoolClass`, `Section`, `Staff` entities + migration
- [x] Grade/class/section/staff commands, queries, endpoints
- [x] Overlap + capacity validation, unit/integration tests

---

# L4 TASK: CreateGradeEntity ✅

- [x] **3.11–3.16** Domain, migration, repository, command, endpoints

---

# L4 TASK: CreateClassCommand ✅

- [x] **3.17–3.20** Class/section entities, commands, `ListClassesQuery`, endpoints

---

# L4 TASK: CreateStaffCommand ✅

- [x] **3.21–3.24** Staff entity, create/link commands, list query, endpoints

---

# L4 TASK: CreateAcademicYearCommand ✅

- [x] **3.1–3.7** Repository, commands, overlap validation, tests

---

# L4 TASK: SetCurrentAcademicYearCommand ✅

- [x] **3.8–3.10** Set-current command + endpoint

---

# L4 TASK: CreateSubjectEntity (Next)

**Points:** 2 | **Permission:** `institution.subjects.manage`

## L5 Subtasks

- [ ] **3.25** Domain entities `Subject`, `ClassSubject` + migration
- [ ] **3.26** `CreateSubjectCommand`, `AssignSubjectToClassCommand`
- [ ] **3.27** Endpoints under `Endpoints/Institution/SubjectEndpoints.cs`

---

# L4 TASK: AppShellLayoutComponent (Frontend — In progress)

**Points:** 5 | **Stack:** Angular 19

## L5 Subtasks

- [x] **3.25** `ng new` scaffold under `frontend/`
- [x] **3.26** `AppShellLayoutComponent` — sidebar + topbar + router-outlet
- [x] **3.27** `SidebarNavigationComponent` — permission-filtered menu items
- [x] **3.28** `PermissionGuard` + `HasPermissionDirective` using `/api/v1/roles/me/permissions` + `auth/me`
- [x] **3.29** `AuthService`, login page, token refresh interceptor
- [ ] **3.30+** Grades, staff, custom fields, roles, audit UI pages

---

# L4 TASK: AcademicYearListComponent (partial)

## L5 Subtasks

- [x] **3.30** `InstitutionApiService` (hand-rolled)
- [x] **3.31** List page bound to `GET /api/v1/academic-years`
- [x] **3.32** Create form → `POST /api/v1/academic-years`
- [x] **3.33** Set-current action → `POST …/set-current`

---

## Sprint 3 Exit Checklist

| # | Criterion | Verified |
|---|---|---|
| 1 | Tenant provisioning creates tenant + default roles + admin atomically | [ ] |
| 2 | Tenant isolation verified after provisioning | [ ] |
| 3 | Academic year CRUD with overlap validation | [x] |
| 4 | Grade → Class → Section hierarchy CRUD | [x] |
| 5 | Staff linked to user accounts | [x] |
| 6 | Admin shell: sidebar, topbar, role-filtered nav | [x] |
| 7 | Permission matrix UI assigns permissions to roles | [ ] |
| 8 | Shared data table wrapper reusable | [ ] |
| 9 | All institution endpoints have RBAC + audit | [x] |
| 10 | OpenAPI spec generated for frontend service sync | [ ] |

---

## Sprint 3 → Sprint 4 Handoff

- `CreateStudentEntity` can reference grades, classes, sections
- Enrollment can use current academic year (`IsCurrent`)
- Admin data table wrapper ready for student list
- Scope resolver can filter students by class assignment
