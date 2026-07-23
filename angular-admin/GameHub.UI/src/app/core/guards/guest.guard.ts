import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AppSessionService } from '@shared/common/session/app-session.service';

export const guestGuard: CanActivateFn = () => {
  const session = inject(AppSessionService);
  const router = inject(Router);

  if (session.user) {
    router.navigate(['/app/main/dashboard']);
    return false;
  }

  return true;
};
