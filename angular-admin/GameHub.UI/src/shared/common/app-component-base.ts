import { Injector } from '@angular/core';
import { PermissionCheckerService } from '@eaf/auth/permission-checker.service';
import { FeatureCheckerService } from '@eaf/features/feature-checker.service';
import { LocalizationService } from '@eaf/localization/localization.service';
import { MessageService } from '@eaf/message/message.service';
import { EafMultiTenancyService } from '@eaf/multi-tenancy/eaf-multi-tenancy.service';
import { NotifyService } from '@eaf/notify/notify.service';
import { SettingService } from '@eaf/settings/setting.service';
import { AppConsts } from '@shared/AppConsts';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import { AppSessionService } from '@shared/common/session/app-session.service';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';
import { DataTableHelper } from '@shared/helpers/DataTableHelper';
import { UiCustomizationSettingsDto } from '@shared/service-proxies/service-proxies';

export abstract class AppComponentBase {
  localizationSourceName = AppConsts.localization.defaultLocalizationSourceName;
  localizationSourceNameEaf = AppConsts.localization.defaultLocalizationSourceNameEaf;
  LocalizationSourceNameAbp = AppConsts.localization.defaultLocalizationSourceNameAbp;
  LocalizationSourceNameAbpWeb = AppConsts.localization.defaultLocalizationSourceNameAbpWeb;
  LocalizationSourceNameAbpZero = AppConsts.localization.defaultLocalizationSourceNameAbpZero;
  LocalizationSourceNameEafAzureActiveDirectory = AppConsts.localization.defaultLocalizationSourceNameEafAzureActiveDirectory;
  LocalizationSourceNameEafLdap = AppConsts.localization.defaultLocalizationSourceNameEafLdap;

  localization: LocalizationService;
  permission: PermissionCheckerService;
  feature: FeatureCheckerService;
  notify: NotifyService;
  setting: SettingService;
  message: MessageService;
  multiTenancy: EafMultiTenancyService;
  appSession: AppSessionService;
  dataTableHelper: DataTableHelper;
  ui: AppUiCustomizationService;
  appUrlService: AppUrlService;

  constructor(injector: Injector) {
    this.localization = injector.get(LocalizationService);
    this.permission = injector.get(PermissionCheckerService);
    this.feature = injector.get(FeatureCheckerService);
    this.notify = injector.get(NotifyService);
    this.setting = injector.get(SettingService);
    this.message = injector.get(MessageService);
    this.multiTenancy = injector.get(EafMultiTenancyService);
    this.appSession = injector.get(AppSessionService);
    this.ui = injector.get(AppUiCustomizationService);
    this.appUrlService = injector.get(AppUrlService);
    this.dataTableHelper = new DataTableHelper();
  }

  l(key: string, ...args: any[]): string {
    return this.ls(this.localizationSourceName, key, ...args);
  }

  ls(sourcename: string, key: string, ...args: any[]): string {
    let localizedText = this.localization.localize(key, this.localizationSourceName);

    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.localizationSourceNameEaf);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameAbp);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameAbpWeb);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameAbpZero);
    if (!localizedText || localizedText == key)
      localizedText = this.localization.localize(key, this.LocalizationSourceNameEafAzureActiveDirectory);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, this.LocalizationSourceNameEafLdap);
    if (!localizedText || localizedText == key) localizedText = this.localization.localize(key, sourcename);

    args.unshift(localizedText);

    return eaf.utils.formatString.call(null, ...args);
  }

  isGranted(permissionName: string): boolean {
    return this.permission.isGranted(permissionName);
  }

  isGrantedAny(...permissions: string[]): boolean {
    if (!permissions) {
      return false;
    }

    for (const permission of permissions) {
      if (this.isGranted(permission)) {
        return true;
      }
    }

    return false;
  }

  s(key: string): string {
    return eaf.setting.get(key);
  }

  appRootUrl(): string {
    return this.appUrlService.appRootUrl;
  }

  get currentTheme(): UiCustomizationSettingsDto {
    return this.appSession.theme;
  }
}
