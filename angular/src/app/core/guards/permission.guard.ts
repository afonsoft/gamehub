import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from '../auth/token.service';

export const PermissionGuard = (requiredPermission: string): CanActivateFn => {
  return () => {
    const tokenService = inject(TokenService);
    const router = inject(Router);

    if (tokenService.isInRole(requiredPermission)) {
      return true;
    }

    return router.createUrlTree(['/unauthorized']);
  };
};
