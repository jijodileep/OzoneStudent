import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { UserPermissionsResult } from '../models/auth.models';
import { AuthService } from './auth.service';
import { EnvironmentConfigService } from './environment-config.service';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly env = inject(EnvironmentConfigService);

  readonly permissions = computed(
    () => this.auth.currentUser()?.permissions ?? [],
  );

  hasPermission(code: string): boolean {
    return this.permissions().includes(code);
  }

  hasAnyPermission(codes: readonly string[]): boolean {
    const granted = this.permissions();
    return codes.some((code) => granted.includes(code));
  }

  refreshFromApi(): Observable<UserPermissionsResult> {
    return this.http
      .get<UserPermissionsResult>(`${this.env.apiBaseUrl}/roles/me/permissions`)
      .pipe(
        tap((result) => {
          const current = this.auth.currentUser();
          if (!current) {
            return;
          }

          this.auth.currentUser.set({
            ...current,
            roles: result.roles,
            permissions: result.permissions,
            scopeType: result.scopeType,
            scopeIds: result.scopeIds,
          });
        }),
      );
  }
}
