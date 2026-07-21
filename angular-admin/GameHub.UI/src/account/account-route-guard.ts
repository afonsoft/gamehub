import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { AppSessionService } from '@shared/common/session/app-session.service';

@Injectable()
export class AccountRouteGuard implements CanActivate {
  constructor(
    private readonly _permissionChecker: PermissionCheckerService,
    private readonly _router: Router,
    private readonly _sessionService: AppSessionService,
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (route.queryParams['ss'] && route.queryParams['ss'] === 'true') {
      return true;
    }

    if (this._sessionService.user) {
      this._router.navigate([this.selectBestRoute()]);
      return false;
    }

    return true;
  }

  selectBestRoute(): string {
    if (this._permissionChecker.isGranted('Pages.Dashboard')) {
      return '/app/main/dashboard';
    }

    return '/app/notifications';
  }
}
