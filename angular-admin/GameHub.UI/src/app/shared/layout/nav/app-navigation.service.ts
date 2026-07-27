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
      new AppMenuItem('Dashboard', 'Pages.Dashboard', 'la la-dashboard', '/app/main/dashboard'),
      new AppMenuItem('GameHub', 'Pages.GameHubDashboard.View', 'la la-gamepad', '/app/main/gamehub/dashboard'),
      new AppMenuItem('Games', 'Pages.Games.View', 'la la-th-large', '/app/main/gamehub/games'),
      new AppMenuItem('Uploads', 'Pages.Builds.View', 'la la-cloud-upload', '/app/main/gamehub/uploads'),
      new AppMenuItem('Inspector', 'Pages.Builds.View', 'la la-search', '/app/main/gamehub/inspector'),
      new AppMenuItem('Moderation', 'Pages.Moderation.View', 'la la-shield', '/app/main/gamehub/moderation'),
      new AppMenuItem('Playtests', 'Pages.Moderation.View', 'la la-play', '/app/main/gamehub/playtests'),
      new AppMenuItem('Categories', 'Pages.Categories.Manage', 'la la-list', '/app/main/gamehub/categories'),
      new AppMenuItem('Tags', 'Pages.Tags.Manage', 'la la-tags', '/app/main/gamehub/tags'),
      new AppMenuItem('Reports', 'Pages.Reports.Manage', 'la la-warning', '/app/main/gamehub/reports'),
      new AppMenuItem('Feature Flags', 'Pages.GameHubDashboard.FeatureFlags', 'la la-cog', '/app/main/gamehub/dashboard/flags'),
      new AppMenuItem('Audit Log', 'Pages.GameHubDashboard.AuditLog', 'la la-folder-open', '/app/main/gamehub/dashboard/audit'),
      new AppMenuItem('Users', 'Pages.Users.Manage', 'la la-users', '/app/main/gamehub/users'),
      new AppMenuItem('Tenants', 'Pages.Tenants', 'la la-building', '/app/admin/tenants'),
    ]);
  }

  getAdminMenu(): AppMenu {
    return new AppMenu('AdminMenu', 'AdminMenu', [
      new AppMenuItem('Roles', 'Pages.Administration.Roles', 'la la-briefcase', '/app/admin/roles'),
      new AppMenuItem('Users', 'Pages.Administration.Users', 'la la-users', '/app/admin/users'),
      new AppMenuItem('Languages', 'Pages.Administration.Languages', 'la la-globe', '/app/admin/languages'),
      new AppMenuItem('AuditLogs', 'Pages.Administration.AuditLogs', 'la la-folder-open', '/app/admin/auditLogs'),
      new AppMenuItem('VisualSettings', 'Pages.Administration.UiCustomization', 'la la-desktop', '/app/admin/ui-customization'),
      new AppMenuItem('Maintenance', 'Pages.Administration.Maintenance', 'la la-cogs', '/app/admin/maintenance'),
      new AppMenuItem('Settings', 'Pages.Administration.Settings', 'la la-cog', '/app/admin/settings'),
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
