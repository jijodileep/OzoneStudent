import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../services/permission.service';

export const permissionGuard: CanActivateFn = (route) => {
  const permissions = inject(PermissionService);
  const router = inject(Router);
  const required = route.data['permission'] as string | string[] | undefined;

  if (!required) {
    return true;
  }

  const codes = Array.isArray(required) ? required : [required];
  if (permissions.hasAnyPermission(codes)) {
    return true;
  }

  return router.createUrlTree(['/dashboard']);
};
