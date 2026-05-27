import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { isApiRequest } from '../utils/api-url.util';
import { AuthService } from '../services/auth.service';
import { EnvironmentConfigService } from '../services/environment-config.service';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const env = inject(EnvironmentConfigService);

  if (!isApiRequest(req.url, env.apiBaseUrl)) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: {
        'X-Tenant-Slug': auth.getTenantSlug(),
      },
    }),
  );
};
