import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { TokenService } from './token.service';

export const developerGuard: CanActivateFn = () => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  const roles = tokenService.getRoles().map(r => r.toLowerCase());
  if (roles.includes('developer') || roles.includes('admin')) {
    return true;
  }

  return router.parseUrl('/');
};
