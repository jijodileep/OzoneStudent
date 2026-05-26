# Universal Logging Policy
## Everything Must Be Logged

**Principle:** No significant platform action occurs without a log record. If it happened, it must be traceable — who, what, when, where, and outcome.

This policy applies to **all modules**, **all clients** (Admin Web, Parent App, Student App), and **all environments** (except local dev may reduce read-audit volume via config).

---

## Two Logging Layers

| Layer | Storage | Purpose | Retention |
|---|---|---|---|
| **Audit Log** | PostgreSQL `audit_logs` (append-only) | Compliance, forensics, admin viewer | 7 years (configurable) |
| **Operational Log** | Serilog → Seq/Elasticsearch/console | Debugging, monitoring, performance | 90 days hot, then archive |

Both layers must include: `TenantId`, `UserId`, `CorrelationId`, `Timestamp`, `IpAddress`, `UserAgent`.

---

## Layer 1 — Audit Log (Mandatory Events)

### Category A: Authentication & Session (always audit)

| Event | Action | Before/After |
|---|---|---|
| Login success | `Login` | — |
| Login failure | `LoginFailed` | Reason (invalid password, locked, invalid tenant) |
| Logout | `Logout` | — |
| Token refresh | `TokenRefresh` | — |
| Token refresh failure | `TokenRefreshFailed` | Reason |
| Password change | `PasswordChange` | — |
| Password reset requested | `PasswordResetRequest` | Email (masked) |
| Password reset completed | `PasswordResetComplete` | — |
| Account locked | `AccountLocked` | Attempt count |
| Invitation sent | `UserInvited` | Email (masked), role |
| Invitation accepted | `InvitationAccepted` | — |
| MFA enabled/disabled | `MfaChange` | Before/after state (Phase 2) |

### Category B: Authorization & RBAC (always audit)

| Event | Action |
|---|---|
| Role created/updated/deleted | `Create` / `Update` / `Delete` |
| Permission assigned/removed from role | `PermissionChange` |
| User role assigned/removed | `PermissionChange` |
| Scope changed | `PermissionChange` |
| **Permission denied** (403) | `AccessDenied` — user, permission, resource |
| Super admin action | `SuperAdminAction` — all platform operations |

### Category C: Data Writes (always audit — every command)

Every `ICommand` that mutates data:

| Event | Action | Payload |
|---|---|---|
| Entity created | `Create` | AfterJson |
| Entity updated | `Update` | BeforeJson + AfterJson |
| Entity soft-deleted | `Delete` | BeforeJson |
| Bulk operation | `BulkUpdate` / `BulkDelete` | Count + filter criteria |
| Status change | `StatusChange` | Before/after status |

**Modules covered:** Institution, Students, Attendance, Fees, Finance, LMS, Notifications (templates), Social (posts), Platform (tenants).

### Category D: Sensitive Data Access (always audit)

Reads that expose PII or financial data must be audit-logged (not just Serilog):

| Event | Action | When |
|---|---|---|
| Single student profile view | `Read` | `GET /students/{id}` |
| Student list export | `Export` | Any bulk student data export |
| Fee ledger view | `Read` | Parent/accountant viewing ledger |
| Payment receipt download | `Export` | PDF receipt download |
| Document download | `Export` | Student document, ID card |
| Report generated | `Export` | Any report PDF/Excel |
| Audit log export | `Export` | Meta-audit (export is itself logged) |
| Cross-tenant access (super admin) | `SuperAdminRead` | Any super-admin data access |

**Non-sensitive reads** (e.g. grade list, subject list) → operational log only, not audit DB.

### Category E: Financial Operations (always audit — highest priority)

| Event | Action |
|---|---|
| Invoice generated | `Create` |
| Invoice cancelled | `StatusChange` |
| Payment recorded | `Create` |
| Payment voided | `Reverse` |
| Concession applied/approved | `Update` |
| Journal entry created | `Create` |
| Journal entry posted | `Post` |
| Journal entry reversed | `Reverse` |
| Receipt generated | `Export` |

Financial audit entries are **never purged** before 7-year retention minimum.

### Category F: Attendance & Academic (always audit)

| Event | Action |
|---|---|
| Attendance marked (bulk) | `BulkUpdate` — class, date, count changed |
| Attendance record edited | `Update` — before/after status |
| Leave submitted | `Create` |
| Leave approved/rejected | `StatusChange` |
| Grade/mark changed | `Update` |
| Report card published | `StatusChange` |

### Category G: Files & Documents (always audit)

| Event | Action |
|---|---|
| File uploaded | `Create` — filename, size, entity link |
| File downloaded | `Export` — who accessed which document |
| File deleted | `Delete` |

### Category H: Notifications (always audit)

| Event | Action |
|---|---|
| Notification sent | `Create` — channel, template, recipient (masked) |
| Notification failed | `Failed` — error reason |
| Broadcast sent | `BulkCreate` — recipient count |
| Template changed | `Update` |

### Category I: Integration Events (always audit)

| Event | Action |
|---|---|
| Event published to outbox | `EventPublished` — EventId, EventType |
| Event consumed | `EventConsumed` — EventId, handler, duration |
| Event failed (retry) | `EventFailed` — EventId, error |
| Event dead-lettered | `EventDeadLettered` — EventId |

### Category J: Platform & Tenant Lifecycle (always audit)

| Event | Action |
|---|---|
| Tenant created | `Create` |
| Tenant suspended/activated | `StatusChange` |
| Tenant plan changed | `Update` |
| Tenant settings changed | `Update` — before/after JSON |
| Feature flag toggled | `Update` |
| Impersonation started/ended | `Impersonation` (Phase 2) |

### Category K: Background Jobs (always audit)

| Event | Action |
|---|---|
| Job started | `JobStarted` — job name, parameters |
| Job completed | `JobCompleted` — duration, records processed |
| Job failed | `JobFailed` — error |

Examples: outbox publisher, fee overdue scan, absentee notification, report aggregation, backup job.

### Category L: Mobile App Actions (always audit)

Parent App and Student App actions audit through the same API — no separate mobile log silo:

| Event | Action |
|---|---|
| Parent views child attendance | `Read` |
| Parent views fee ledger | `Read` |
| Student submits assignment | `Create` |
| Push token registered | `Update` |

---

## Layer 2 — Operational Log (Serilog — Every Request)

Every HTTP request is logged via middleware (regardless of audit category):

```json
{
  "CorrelationId": "abc-123",
  "TenantId": "guid",
  "UserId": "guid",
  "Method": "POST",
  "Path": "/api/v1/students",
  "StatusCode": 201,
  "ElapsedMs": 45,
  "IpAddress": "1.2.3.4",
  "UserAgent": "..."
}
```

Also log at `Information` or `Warning`:
- Validation failures
- Business rule failures (`Result.Failure`)
- Unhandled exceptions (with stack in non-production)
- Slow requests (> 2s at Warning, > 5s at Error)
- RabbitMQ publish/consume
- Redis cache miss/hit (Debug only)
- EF Core slow queries (> 500ms)

**Never log in operational logs:**
- Passwords, tokens, refresh tokens
- Full credit card numbers
- Unmasked PII in production (mask: email → `j***@school.com`)

---

## AuditLog Schema (Extended)

```text
audit_logs
├── id                 UUID PK
├── tenant_id          UUID nullable (null for platform/super-admin)
├── user_id            UUID nullable (null for system jobs)
├── action             VARCHAR(50)   -- Create, Update, Delete, Read, Export, Login, etc.
├── category           VARCHAR(50)   -- Auth, Rbac, Student, Fee, Finance, etc.
├── entity_type        VARCHAR(100)  -- Student, FeePayment, etc.
├── entity_id          UUID nullable
├── description        TEXT          -- Human-readable summary
├── before_json        JSONB nullable
├── after_json         JSONB nullable
├── metadata_json      JSONB nullable -- Extra context (IP, user agent, filters, counts)
├── ip_address         VARCHAR(45)
├── user_agent         TEXT
├── correlation_id     VARCHAR(100)
├── source             VARCHAR(20)   -- Api, Mobile, Job, EventHandler
├── outcome            VARCHAR(20)   -- Success, Failed, Denied
├── created_at         TIMESTAMPTZ   -- Immutable, never update
```

**Rules:**
- Append-only — no UPDATE or DELETE on `audit_logs`
- No API endpoint to delete audit records
- Audit writes bypass AuditBehavior (direct insert) to prevent infinite loop

---

## Implementation Checklist (Every Feature)

When implementing any feature, verify:

- [ ] Write command → `AuditBehavior` or explicit `IAuditService.LogAsync`
- [ ] Sensitive read query → `AuditReadBehavior` or explicit read audit
- [ ] API endpoint → request logged via `RequestLoggingMiddleware`
- [ ] Permission denied → `AccessDenied` audit entry
- [ ] Export/download → `Export` audit entry
- [ ] Background job → start/complete/fail audit entries
- [ ] Integration event → publish/consume logged
- [ ] File upload/download → audit entry
- [ ] Unit test confirms audit entry created
- [ ] PII masked in operational logs

---

## MediatR Pipeline Order (Logging-Aware)

```
1. LoggingBehavior          → log command/query start
2. ValidationBehavior       → log validation failures
3. AuthorizationBehavior    → log AccessDenied on failure
4. TenantBehavior
5. AuditReadBehavior        → log sensitive reads (queries)
6. AuditBehavior            → log writes (commands)
7. Handler
8. LoggingBehavior          → log completion + elapsed ms
```

---

## Module-by-Module Audit Coverage Matrix

| Module | Writes | Sensitive Reads | Exports | Events | Jobs |
|---|---|---|---|---|---|
| Identity & Auth | ✅ | ✅ (profile) | — | ✅ | — |
| RBAC | ✅ | ✅ (permissions) | — | ✅ | — |
| Audit | — | ✅ (log view) | ✅ | — | ✅ archival |
| Institution | ✅ | — | — | ✅ | — |
| Students | ✅ | ✅ | ✅ | ✅ | ✅ import |
| Attendance | ✅ | ✅ | ✅ | ✅ | ✅ absentee |
| Fees | ✅ | ✅ | ✅ | ✅ | ✅ overdue |
| Finance | ✅ | ✅ | ✅ | ✅ | ✅ ledger recalc |
| LMS | ✅ | ✅ | ✅ | ✅ | ✅ report cards |
| Notifications | ✅ | ✅ (inbox) | — | ✅ | ✅ dispatch |
| Reporting | — | ✅ | ✅ | — | ✅ aggregation |
| Social | ✅ | — | — | ✅ | — |
| Platform/Super Admin | ✅ | ✅ | ✅ | ✅ | — |

---

## Configuration

```json
{
  "Audit": {
    "EnableReadAudit": true,
    "ReadAuditEntities": ["Student", "Guardian", "FeePayment", "FeeInvoice", "JournalEntry"],
    "MaskPiiInOperationalLogs": true,
    "RetentionDays": 2555,
    "ArchiveAfterDays": 365
  }
}
```

Local dev: set `EnableReadAudit: false` to reduce noise — **never disable in staging/production**.

---

## Related Files

- [`.cursor/rules/backend-audit.mdc`](../.cursor/rules/backend-audit.mdc)
- [`.cursor/rules/module-audit-logging.mdc`](../.cursor/rules/module-audit-logging.mdc)
- [`student_management_saas_complete_module_plan.md`](../student_management_saas_complete_module_plan.md) — Module 3
