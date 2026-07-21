import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { Injector, Injectable } from '@angular/core';
import { LocalizationService } from '@eaf/localization/localization.service';
import { AppMenu } from './app-menu';
import { AppMenuItem } from './app-menu-item';

@Injectable()
export class AppNavigationService {
  localization: LocalizationService;
  constructor(
    injector: Injector,
    private readonly _permissionCheckerService: PermissionCheckerService,
    private readonly _appSessionService: AppSessionService,
  ) {
    this.localization = injector.get(LocalizationService);
  }

  getMenu(): AppMenu {
    return new AppMenu('MainMenu', 'MainMenu', [
      new AppMenuItem('Dashboard', 'Pages.Dashboard', 'flaticon-line-graph', '/app/main/dashboard'),
      new AppMenuItem('Tenants', 'Pages.Tenants', 'flaticon-squares-4', '/app/admin/tenants'),
      new AppMenuItem('Airplanes', 'Pages.Airplanes', 'flaticon-paper-plane', '/app/main/airplanes'),
    ]);
  }

  getAdminMenu(): AppMenu {
    return new AppMenu('AdminMenu', 'AdminMenu', [
      new AppMenuItem('Roles', 'Pages.Administration.Roles', 'flaticon-suitcase', '/app/admin/roles'),
      new AppMenuItem('Users', 'Pages.Administration.Users', 'flaticon-users', '/app/admin/users'),
      new AppMenuItem('Languages', 'Pages.Administration.Languages', 'flaticon-tabs', '/app/admin/languages'),
      new AppMenuItem('AuditLogs', 'Pages.Administration.AuditLogs', 'flaticon-folder-1', '/app/admin/auditLogs'),
      new AppMenuItem('VisualSettings', 'Pages.Administration.UiCustomization', 'flaticon-imac', '/app/admin/ui-customization'),
      new AppMenuItem('Maintenance', 'Pages.Administration.Maintenance', 'flaticon-lock', '/app/admin/maintenance'),
      new AppMenuItem('Settings', 'Pages.Administration.Settings', 'flaticon-settings', '/app/admin/settings'),
      new AppMenuItem('Parameters', 'Pages.Administration.Maintenance', 'flaticon-lock', '/app/admin/parameters'),
      new AppMenuItem('Jobs', 'Pages.Administration.HangfireDashboard', 'flaticon-line-graph', '/app/admin/hangfire'),
    ]);
  }

  checkChildMenuItemPermission(menuItem): boolean {
    for (const subMenuItem of menuItem.items) {
      if (subMenuItem.permissionName && this._permissionCheckerService.isGranted(subMenuItem.permissionName)) {
        return true;
      } else if (subMenuItem.items?.length) {
        return this.checkChildMenuItemPermission(subMenuItem);
      }
    }

    return false;
  }

  showMenuItem(menuItem: AppMenuItem): boolean {
    let hideMenuItem = false;

    if (menuItem.requiresAuthentication && !this._appSessionService.user) {
      hideMenuItem = true;
    }

    if (menuItem.permissionName && !this._permissionCheckerService.isGranted(menuItem.permissionName)) {
      hideMenuItem = true;
    }

    if (menuItem.hasFeatureDependency() && !menuItem.featureDependencySatisfied()) {
      hideMenuItem = true;
    }

    if (!hideMenuItem && menuItem.items?.length) {
      return this.checkChildMenuItemPermission(menuItem);
    }

    return !hideMenuItem;
  }
}
