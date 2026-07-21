import { CompilerOptions, NgModuleRef, Type } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';
import { DynamicResourcesHelper } from '@shared/helpers/DynamicResourcesHelper';
import { XmlHttpRequestHelper } from '@shared/helpers/XmlHttpRequestHelper';
import * as _ from 'lodash';
import * as moment from 'moment';

import { environment } from './environments/environment';
import { UrlHelper } from './shared/helpers/UrlHelper';

export class AppPreBootstrap {
  private static storageService: StorageService;

  static Init(storageService: StorageService) {
    this.storageService = storageService;
  }

  static run(appRootUrl: string, callback: () => void, resolve: any, reject: any): void {
    AppPreBootstrap.getApplicationConfig(appRootUrl, () => {
      const queryStringObj = UrlHelper.getQueryParameters();

      if (queryStringObj.impersonationToken) {
        AppPreBootstrap.impersonatedAuthenticate(queryStringObj.impersonationToken, queryStringObj.tenantId, () => {
          AppPreBootstrap.getUserConfiguration(callback);
        });
      } else {
        AppPreBootstrap.getUserConfiguration(callback);
      }
    });
  }

  static bootstrap<TM>(moduleType: Type<TM>, compilerOptions?: CompilerOptions | CompilerOptions[]): Promise<NgModuleRef<TM>> {
    return platformBrowserDynamic().bootstrapModule(moduleType, compilerOptions);
  }

  private static getApplicationConfig(appRootUrl: string, callback: () => void) {
    const env = (window as any).env || {};
    if (env.remoteServiceBaseUrl) {
      const currentOrigin = window.location.origin;
      AppConsts.remoteServiceBaseUrlFormat = env.remoteServiceBaseUrl;
      AppConsts.remoteServiceBaseUrl = env.remoteServiceBaseUrl;
      AppConsts.appBaseUrlFormat = env.appBaseUrl || currentOrigin;
      AppConsts.appBaseUrl = env.appBaseUrl || currentOrigin;
      AppConsts.localeMappings = env.localeMappings || [{ from: 'pt-BR', to: 'pt' }];
      callback();
      return;
    }

    const type = 'GET';
    const url = appRootUrl + 'assets/' + environment.appConfig;
    const customHeaders = [
      {
        name: 'Abp.TenantId',
        value: eaf.multiTenancy.getTenantIdCookie() + '',
      },
    ];

    XmlHttpRequestHelper.ajax(type, url, customHeaders, null, result => {
      const currentOrigin = window.location.origin;
      let appBaseUrl = result.appBaseUrl;

      if (appBaseUrl?.includes('localhost') && !currentOrigin.includes('localhost')) {
        appBaseUrl = currentOrigin;
      }

      AppConsts.appBaseUrlFormat = appBaseUrl;
      AppConsts.remoteServiceBaseUrlFormat = result.remoteServiceBaseUrl;
      AppConsts.localeMappings = result.localeMappings;

      AppConsts.appBaseUrl = appBaseUrl;
      AppConsts.remoteServiceBaseUrl = result.remoteServiceBaseUrl;

      callback();
    });
  }

  private static getCurrentClockProvider(currentProviderName: string): eaf.timing.IClockProvider {
    if (currentProviderName === 'unspecifiedClockProvider') {
      return eaf.timing.unspecifiedClockProvider;
    }

    if (currentProviderName === 'utcClockProvider') {
      return eaf.timing.utcClockProvider;
    }

    return eaf.timing.localClockProvider;
  }

  private static impersonatedAuthenticate(impersonationToken: string, tenantId: number, callback: () => void): void {
    eaf.multiTenancy.setTenantIdCookie(tenantId);
    const cookieLangValue = this.storageService.getCookieValue('Abp.Localization.CultureName');

    const requestHeaders = {
      '.AspNetCore.Culture': 'c=' + cookieLangValue + '|uic=' + cookieLangValue,
      'Abp.TenantId': eaf.multiTenancy.getTenantIdCookie(),
      'Abp.Localization.CultureName': cookieLangValue,
      'Accept-Language': cookieLangValue,
    };

    XmlHttpRequestHelper.ajax(
      'POST',
      AppConsts.remoteServiceBaseUrl + '/api/TokenAuth/ImpersonatedAuthenticate?impersonationToken=' + impersonationToken,
      requestHeaders,
      null,
      response => {
        const result = response.result;
        eaf.auth.setToken(result.accessToken);
        AppPreBootstrap.setEncryptedTokenCookie(result.encryptedAccessToken);
        location.search = '';
        callback();
      },
    );
  }

  private static getUserConfiguration(callback: () => void): any {
    let cookieLangValue = this.storageService.getCookieValue('Abp.Localization.CultureName');

    if (cookieLangValue === null || cookieLangValue === '') {
      cookieLangValue = 'pt-BR';
      this.storageService.setCookieValue(
        'Abp.Localization.CultureName',
        cookieLangValue,
        new Date(Date.now() + 5 * 365 * 86400000), // 5 year
        eaf.appPath,
      );
    }

    const token = this.storageService.getCookieValue(eaf.auth.tokenCookieName);

    const requestHeaders = {
      '.AspNetCore.Culture': 'c=' + cookieLangValue + '|uic=' + cookieLangValue,
      'Abp.TenantId': eaf.multiTenancy.getTenantIdCookie(),
      'Abp.Localization.CultureName': cookieLangValue,
      'Accept-Language': cookieLangValue,
    };

    if (token) {
      requestHeaders['Authorization'] = 'Bearer ' + token;
    }

    return XmlHttpRequestHelper.ajax(
      'GET',
      AppConsts.remoteServiceBaseUrl + '/AbpUserConfiguration/GetAll',
      requestHeaders,
      null,
      response => {
        const result = response.result;

        _.merge(eaf, result);

        eaf.clock.provider = this.getCurrentClockProvider(result.clock.provider);

        moment.locale(eaf.localization.currentLanguage.name);
        (window as any).moment.locale(eaf.localization.currentLanguage.name);

        if (eaf.clock.provider.supportsMultipleTimezone) {
          moment.tz.setDefault(eaf.timing.timeZoneInfo.iana.timeZoneId);
          (window as any).moment.tz.setDefault(eaf.timing.timeZoneInfo.iana.timeZoneId);
        }

        eaf.event.trigger('eaf.dynamicScriptsInitialized');

        DynamicResourcesHelper.loadResources(callback);
      },
    );
  }

  private static setEncryptedTokenCookie(encryptedToken: string) {
    this.storageService.setCookieValue(
      AppConsts.authorization.encrptedAuthTokenName,
      encryptedToken,
      new Date(Date.now() + 365 * 86400000), //1 year
      eaf.appPath,
    );
  }
}
