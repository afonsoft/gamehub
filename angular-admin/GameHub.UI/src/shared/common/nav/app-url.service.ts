import { Injectable } from '@angular/core';
import { AppConsts } from '@shared/AppConsts';
import { AppSessionService } from '@shared/common/session/app-session.service';

@Injectable()
export class AppUrlService {
  static readonly tenancyNamePlaceHolder = '{TENANCY_NAME}';

  constructor(private readonly _appSessionService: AppSessionService) {}

  get appRootUrl(): string {
    if (this._appSessionService.tenant) {
      return this.getAppRootUrlOfTenant(this._appSessionService.tenant.tenancyName);
    } else {
      return this.getAppRootUrlOfTenant(null);
    }
  }

  /**
   * Returning url ends with '/'.
   */
  getAppRootUrlOfTenant(tenancyName?: string): string {
    let baseUrl = this.ensureEndsWith(AppConsts.appBaseUrlFormat, '/');

    //Add base href if it is not configured in appconfig.json
    if (!baseUrl.includes(AppConsts.appBaseHref)) {
      baseUrl = baseUrl + this.removeFromStart(AppConsts.appBaseHref, '/');
    }

    if (!baseUrl.includes(AppUrlService.tenancyNamePlaceHolder)) {
      return baseUrl;
    }

    if (baseUrl.includes(AppUrlService.tenancyNamePlaceHolder + '.')) {
      baseUrl = baseUrl.replace(AppUrlService.tenancyNamePlaceHolder + '.', AppUrlService.tenancyNamePlaceHolder);
      if (tenancyName) {
        tenancyName = tenancyName + '.';
      }
    }

    if (!tenancyName) {
      return baseUrl.replace(AppUrlService.tenancyNamePlaceHolder, '');
    }

    return baseUrl.replace(AppUrlService.tenancyNamePlaceHolder, tenancyName);
  }

  private ensureEndsWith(str: string, c: string) {
    if (!str.endsWith(c)) {
      str = str + c;
    }

    return str;
  }

  private removeFromEnd(str: string, c: string) {
    if (str.endsWith(c)) {
      str = str.substring(0, str.length - 1);
    }

    return str;
  }

  private removeFromStart(str: string, c: string) {
    if (str.startsWith(c)) {
      str = str.substring(1, str.length);
    }

    return str;
  }
}
