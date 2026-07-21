import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, CanActivateChild, CanLoad, Data, Route, Router, RouterStateSnapshot } from '@angular/router';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { Observable } from 'rxjs';

@Injectable()
export class AppRouteGuard implements CanActivate, CanActivateChild, CanLoad {
  constructor(
    private readonly _permissionChecker: PermissionCheckerService,
    private readonly _router: Router,
    private readonly _sessionService: AppSessionService,
  ) {}

  canActivateInternal(data: Data, state: RouterStateSnapshot): boolean {
    if (!this._sessionService.user) {
      this._router.navigate(['/account/login']);
      return false;
    }

    if (!data?.['permission']) {
      return true;
    }

    if (this._permissionChecker.isGranted(data['permission'])) {
      return true;
    }

    this._router.navigate([this.selectBestRoute()]);
    return false;
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    return this.canActivateInternal(route.data, state);
  }

  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    return this.canActivate(route, state);
  }

  canLoad(route: Route): Observable<boolean> | Promise<boolean> | boolean {
    return this.canActivateInternal(route.data, null);
  }

  selectBestRoute(): string {
    if (!this._sessionService.user) {
      return '/account/login';
    }

    if (this._permissionChecker.isGranted('Pages.Tenant.Dashboard')) {
      return '/app/main/dashboard';
    }

    if (this._permissionChecker.isGranted('Pages.Tenants')) {
      return '/app/admin/tenants';
    }

    if (this._permissionChecker.isGranted('Pages.Administration.Users')) {
      return '/app/admin/users';
    }

    return '/app/notifications';
  }
}
