import { Injectable } from '@angular/core';
import { HostSettingsServiceProxy } from '@shared/service-proxies/service-proxies';
import { AppConsts } from 'shared/AppConsts';

@Injectable()
export class AppAuthenticationService {
  constructor(private readonly _hostSettingService: HostSettingsServiceProxy) {}

  init(): Promise<boolean> {
    return new Promise<boolean>((resolve, reject) => {


      this._hostSettingService.getAllSettings().subscribe(setting => {
        AppConsts.appActiveDirectoryEnabled = setting.azureActiveDirectory.isModuleEnabled && setting.azureActiveDirectory.isEnabled;
        AppConsts.appLdapEnabled = setting.ldap.isModuleEnabled && setting.ldap.isEnabled;
        AppConsts.googleAnalytics = setting.google.analytics;
        AppConsts.googleTagManager = setting.google.tag;
        AppConsts.recaptchaSiteKey = setting.google.recaptchaSiteKey;
        resolve(true);
      });
    });
  }
}
