import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { AppShellComponent } from './layout/app-shell/app-shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login-page/login-page.component').then(
        (m) => m.LoginPageComponent,
      ),
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page/dashboard-page.component').then(
            (m) => m.DashboardPageComponent,
          ),
      },
      {
        path: 'institution/academic-years',
        canActivate: [permissionGuard],
        data: { permission: 'institution.academic-years.manage' },
        loadComponent: () =>
          import('./features/institution/academic-year-list/academic-year-list.component').then(
            (m) => m.AcademicYearListComponent,
          ),
      },
      {
        path: 'institution/grades',
        canActivate: [permissionGuard],
        data: {
          permission: 'institution.classes.manage',
          titleKey: 'nav.grades',
        },
        loadComponent: () =>
          import('./shared/components/placeholder-page/placeholder-page.component').then(
            (m) => m.PlaceholderPageComponent,
          ),
      },
      {
        path: 'institution/staff',
        canActivate: [permissionGuard],
        data: {
          permission: 'institution.staff.manage',
          titleKey: 'nav.staff',
        },
        loadComponent: () =>
          import('./shared/components/placeholder-page/placeholder-page.component').then(
            (m) => m.PlaceholderPageComponent,
          ),
      },
      {
        path: 'institution/custom-fields',
        canActivate: [permissionGuard],
        data: {
          permission: 'institution.staff.fields.manage',
          titleKey: 'nav.customFields',
        },
        loadComponent: () =>
          import('./shared/components/placeholder-page/placeholder-page.component').then(
            (m) => m.PlaceholderPageComponent,
          ),
      },
      {
        path: 'settings/roles',
        canActivate: [permissionGuard],
        data: {
          permission: 'roles.role.read',
          titleKey: 'nav.roles',
        },
        loadComponent: () =>
          import('./shared/components/placeholder-page/placeholder-page.component').then(
            (m) => m.PlaceholderPageComponent,
          ),
      },
      {
        path: 'settings/audit-logs',
        canActivate: [permissionGuard],
        data: {
          permission: 'audit.logs.read',
          titleKey: 'nav.auditLogs',
        },
        loadComponent: () =>
          import('./shared/components/placeholder-page/placeholder-page.component').then(
            (m) => m.PlaceholderPageComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
