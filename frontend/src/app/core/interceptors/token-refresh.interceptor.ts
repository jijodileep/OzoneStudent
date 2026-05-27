import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { isApiRequest } from '../utils/api-url.util';
import { AuthService } from '../services/auth.service';
import { EnvironmentConfigService } from '../services/environment-config.service';

let refreshInFlight = false;

export const tokenRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const env = inject(EnvironmentConfigService);
  const router = inject(Router);

  if (!isApiRequest(req.url, env.apiBaseUrl)) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        return throwError(() => error);
      }

      const path = req.url.includes('://') ? new URL(req.url).pathname : req.url;
      if (
        path.includes('/auth/login') ||
        path.includes('/auth/refresh') ||
        path.includes('/auth/me') ||
        refreshInFlight
      ) {
        return throwError(() => error);
      }

      refreshInFlight = true;
      return auth.refreshTokens().pipe(
        switchMap((success) => {
          refreshInFlight = false;
          if (!success) {
            auth.clearSession();
            if (!router.url.startsWith('/login')) {
              void router.navigate(['/login']);
            }
            return throwError(() => error);
          }

          const token = auth.getAccessToken();
          return next(
            req.clone({
              setHeaders: {
                Authorization: `Bearer ${token ?? ''}`,
              },
            }),
          );
        }),
        catchError((refreshError) => {
          refreshInFlight = false;
          auth.clearSession();
          if (!router.url.startsWith('/login')) {
            void router.navigate(['/login']);
          }
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
