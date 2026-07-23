import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { AppSessionService } from '@shared/common/session/app-session.service';

export const moderatorGuard: CanActivateFn = () => {
  const session = inject(AppSessionService);
  const permissionChecker = inject(PermissionCheckerService);
  const router = inject(Router);

  if (!session.user) {
    router.navigate(['/account/login']);
    return false;
  }

  if (permissionChecker.isGranted('Pages.Moderation.View')) {
    return true;
  }

  router.navigate(['/app/notifications']);
  return false;
};
