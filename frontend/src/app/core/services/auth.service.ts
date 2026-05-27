import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import { CurrentUserDto, LoginResult } from '../models/auth.models';
import { EnvironmentConfigService } from './environment-config.service';

const ACCESS_TOKEN_KEY = 'ss_access_token';
const REFRESH_TOKEN_KEY = 'ss_refresh_token';
const TENANT_SLUG_KEY = 'ss_tenant_slug';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly env = inject(EnvironmentConfigService);
  private readonly router = inject(Router);
  private readonly storage = sessionStorage;

  readonly currentUser = signal<CurrentUserDto | null>(null);
  readonly isAuthenticated = computed(() => !!this.getAccessToken());

  initializeSession(): Observable<CurrentUserDto | null> {
    if (!this.getAccessToken()) {
      this.currentUser.set(null);
      return of(null);
    }

    return this.loadCurrentUser().pipe(
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
    );
  }

  login(email: string, password: string, tenantSlug: string): Observable<void> {
    this.setTenantSlug(tenantSlug);

    return this.http
      .post<LoginResult>(`${this.env.apiBaseUrl}/auth/login`, { email, password })
      .pipe(
        tap((result) => this.storeTokens(result)),
        switchMap(() =>
          this.loadCurrentUser().pipe(
            catchError(() => of(null)),
          ),
        ),
        map(() => void 0),
      );
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    const request$ = refreshToken
      ? this.http.post<void>(`${this.env.apiBaseUrl}/auth/logout`, { refreshToken })
      : of(void 0);

    return request$.pipe(
      catchError(() => of(void 0)),
      tap(() => this.clearSession()),
      tap(() => void this.router.navigate(['/login'])),
      map(() => void 0),
    );
  }

  refreshTokens(): Observable<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return of(false);
    }

    return this.http
      .post<LoginResult>(`${this.env.apiBaseUrl}/auth/refresh`, { refreshToken })
      .pipe(
        tap((result) => this.storeTokens(result)),
        switchMap(() => this.loadCurrentUser()),
        map(() => true),
        catchError(() => {
          this.clearSession();
          return of(false);
        }),
      );
  }

  loadCurrentUser(): Observable<CurrentUserDto> {
    return this.http.get<CurrentUserDto>(`${this.env.apiBaseUrl}/auth/me`).pipe(
      tap((user) => this.currentUser.set(user)),
    );
  }

  getAccessToken(): string | null {
    return this.storage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return this.storage.getItem(REFRESH_TOKEN_KEY);
  }

  getTenantSlug(): string {
    return this.storage.getItem(TENANT_SLUG_KEY) ?? this.env.defaultTenantSlug;
  }

  setTenantSlug(tenantSlug: string): void {
    this.storage.setItem(TENANT_SLUG_KEY, tenantSlug.trim());
  }

  clearSession(): void {
    this.storage.removeItem(ACCESS_TOKEN_KEY);
    this.storage.removeItem(REFRESH_TOKEN_KEY);
    this.currentUser.set(null);
  }

  private storeTokens(result: LoginResult): void {
    if (!result?.accessToken || !result?.refreshToken) {
      throw new Error('Login response did not include tokens.');
    }

    this.storage.setItem(ACCESS_TOKEN_KEY, result.accessToken);
    this.storage.setItem(REFRESH_TOKEN_KEY, result.refreshToken);
  }
}
