# School SaaS — Angular Admin

Angular 19 admin app with Bootstrap 5, ngx-translate (en / ar / hi), Flatpickr date pickers, and server-side data tables.

## Quick start

Prerequisites: Node.js 20+, npm. API on `http://localhost:5275` (CORS allows `http://localhost:4200`).

```bash
cd frontend
npm install
npm start
```

Open http://localhost:4200 and sign in (tenant `demo`, credentials in root [README](../README.md#dev-login)).

The dev server proxies `/api` → `http://localhost:5275` via `proxy.conf.json`.

```bash
npm run build    # production build → dist/frontend
npm test         # unit tests (Karma)
```

## Routes

| Route | Permission | Description |
|-------|------------|-------------|
| `/login` | — | Sign in |
| `/dashboard` | authenticated | Welcome + roles |
| `/institution/academic-years` | `institution.academic-years.manage` | List, search, filter, create (modal), set current |
| `/institution/grades` | `institution.classes.manage` | Placeholder |
| `/institution/staff` | `institution.staff.manage` | Placeholder |
| `/institution/custom-fields` | `institution.staff.fields.manage` | Placeholder |
| `/settings/roles` | `roles.role.read` | Placeholder |
| `/settings/audit-logs` | `audit.logs.read` | Placeholder |

Page headers show **title only** (no descriptive subtitles).

## Project structure

```text
src/app/
  core/
    guards/              auth.guard, permission.guard
    interceptors/        auth, tenant, token-refresh, correlation-id
    services/            auth, locale, permission, environment-config
    utils/               api-url.util, api-error.util
    models/              auth DTOs
  layout/
    app-shell/           navbar, sidebar, language switcher, global modals
    sidebar-navigation/  searchable accordion nav (see shared/models/nav-item.model.ts)
  features/
    auth/login-page/
    dashboard/
    institution/
      academic-year-list/
      create-academic-year-modal/
      services/          institution-api, create-academic-year-modal
  shared/
    components/
      modal/             app-modal (reusable Bootstrap dialog)
      date-picker/       app-date-picker (Flatpickr CVA)
      data-table/        search, filters, sort, client/server pagination
      placeholder-page/
    utils/               date-only.util, table-query.util
    directives/          has-permission
public/i18n/             en.json, ar.json, hi.json
```

## Shared components

### `app-modal`

Reusable dialog shell with projected `[modal-body]` and `[modal-footer]` slots.

```html
<app-modal [open]="isOpen" titleKey="some.title" (dismissed)="close()">
  <div modal-body>...</div>
  <div modal-footer>...</div>
</app-modal>
```

### `app-date-picker`

Form control (`formControlName`) with timezone-safe **date-only** values (`YYYY-MM-DD`).

| Input | Description |
|-------|-------------|
| `inputId` | Label association |
| `placeholder` | Input placeholder |
| `invalid` | Bootstrap invalid state |
| `minDate` / `maxDate` | Linked bounds (`YYYY-MM-DD`) |
| `inline` | Static calendar (optional; modals use popup + `document.body`) |

Uses `parseDateOnly` / `formatDateOnly` from `shared/utils/date-only.util.ts` (no UTC shift).

### `app-data-table-wrapper`

| Input | Description |
|-------|-------------|
| `columns` | Column defs (sort, filter, display) |
| `rows` | Current page rows |
| `serverSide` | `true` for API-backed pagination |
| `totalCount` | Total records (server mode; use signal `input()`) |
| `loading` | Loading state |
| `searchPlaceholder` | i18n key for search box |

Emits `queryChange` with `{ page, pageSize, search, sortBy, sortDirection, filters }`.

Toolbar, table body, and footer use **1.5rem** horizontal padding.

## Create academic year (reusable modal)

Mounted once in `app-shell`. Open from any component:

```typescript
import { CreateAcademicYearModalService } from '../services/create-academic-year-modal.service';

private readonly createModal = inject(CreateAcademicYearModalService);

openCreate(): void {
  this.createModal.open().subscribe((created) => {
    if (created) {
      // created.id, created.name, …
    }
  });
}
```

Form validation: name required, max 100 chars, start/end dates required. Field-level and API errors are translated.

## API errors (i18n)

Backend returns error **keys** in `{ errors: [{ code, message, field? }] }`.

Frontend maps `code` → `errors.{code}` in `public/i18n/*.json` via `core/utils/api-error.util.ts`:

```typescript
extractApiErrors(err)           // from HttpErrorResponse
toErrorTranslationKeys(errors)  // → ['errors.institution.academic_year.name_exists', …]
```

Example keys:

| Code | Translation key |
|------|-----------------|
| `institution.academic_year.name_required` | `errors.institution.academic_year.name_required` |
| `institution.academic_year.name_exists` | `errors.institution.academic_year.name_exists` |
| `institution.academic_year.dates_overlap` | `errors.institution.academic_year.dates_overlap` |

## i18n and RTL

- **Languages:** English, Arabic, Hindi (`LocaleService`, navbar switcher)
- **Files:** `public/i18n/{en,ar,hi}.json`
- **RTL:** Arabic sets `dir="rtl"` and toggles Bootstrap RTL stylesheet (`#bootstrap-rtl`)
- **Flatpickr:** Arabic / Hindi locales loaded in `LocaleService`

## Auth

- Tokens in `sessionStorage`; `AuthService` + interceptors attach Bearer + `X-Tenant-Slug`
- `authGuard` on shell routes; `permissionGuard` on feature routes
- Sidebar uses searchable accordion nav in `nav-item.model.ts`; items filtered by `PermissionService`, empty groups hidden

## Institution API service

`InstitutionApiService` (`features/institution/services/institution-api.service.ts`):

- `listAcademicYears(query)` — server pagination, search, sort, status/current filters
- `createAcademicYear(body)`
- `setCurrentAcademicYear(id)`

List response normalizes camelCase / PascalCase (`totalCount`, `items`).

## UI conventions

- Bootstrap 5 + Bootstrap Icons (no Angular Material)
- Page title + primary action button (no subtitle lines)
- Tables: card layout, padded toolbar/search, hover rows, pill badges
- Modals: backdrop below dialog; date picker calendar `z-index: 1200`

## Environment

`src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: '/api/v1',
  defaultTenantSlug: 'demo',
};
```

## Additional resources

- [Root README](../README.md) — API, Docker, dev login
- [Angular CLI](https://angular.dev/tools/cli)
