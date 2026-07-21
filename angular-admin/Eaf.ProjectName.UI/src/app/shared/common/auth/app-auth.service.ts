import { Injectable } from '@angular/core';
import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';
import { XmlHttpRequestHelper } from '@shared/helpers/XmlHttpRequestHelper';

@Injectable()
export class AppAuthService {
  constructor(private readonly storageService: StorageService) {}

  logout(reload?: boolean, returnUrl?: string): void {
    const customHeaders = {
      'Abp.TenantId': eaf.multiTenancy.getTenantIdCookie(),
      'Authorization': 'Bearer ' + eaf.auth.getToken(),
    };

    XmlHttpRequestHelper.ajax('GET', AppConsts.remoteServiceBaseUrl + '/api/TokenAuth/LogOut', customHeaders, null, () => {
      eaf.auth.clearToken();
      this.storageService.Clear();
      this.storageService.setCookieValue(eaf.auth.tokenCookieName, undefined, undefined, eaf.appPath);
      this.storageService.setCookieValue(AppConsts.authorization.encrptedAuthTokenName, undefined, undefined, eaf.appPath);

      if (reload !== false) {
        if (returnUrl) {
          location.href = returnUrl;
        } else {
          location.href = '';
        }
      }
    });
  }
}
