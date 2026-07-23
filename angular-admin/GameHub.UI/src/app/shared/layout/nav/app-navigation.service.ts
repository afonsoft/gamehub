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
      new AppMenuItem('GameHub', 'Pages.GameHubDashboard.View', 'flaticon-game', '/app/main/gamehub/dashboard'),
      new AppMenuItem('Games', 'Pages.Games.View', 'flaticon-layers', '/app/main/gamehub/games'),
      new AppMenuItem('Uploads', 'Pages.Builds.View', 'flaticon-upload', '/app/main/gamehub/uploads'),
      new AppMenuItem('Moderation', 'Pages.Moderation.View', 'flaticon-shield', '/app/main/gamehub/moderation'),
      new AppMenuItem('Categories', 'Pages.Categories.Manage', 'flaticon-list', '/app/main/gamehub/categories'),
      new AppMenuItem('Tags', 'Pages.Tags.Manage', 'flaticon-tags', '/app/main/gamehub/tags'),
      new AppMenuItem('Users', 'Pages.Users.Manage', 'flaticon-users-1', '/app/main/gamehub/users'),
      new AppMenuItem('Tenants', 'Pages.Tenants', 'flaticon-squares-4', '/app/admin/tenants'),
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
