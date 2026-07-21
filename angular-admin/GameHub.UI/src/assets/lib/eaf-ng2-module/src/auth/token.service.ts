///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>

import { Injectable } from '@angular/core';
import { StorageService } from '@eaf/utils/storage.service';

@Injectable({
  providedIn: 'root',
})
export class TokenService {
  constructor(private readonly storageService: StorageService) {}

  getToken(): string {
    return this.storageService.getCookieValue(eaf.auth.tokenCookieName);
  }

  getTokenCookieName(): string {
    return eaf.auth.tokenCookieName;
  }

  clearToken(): void {
    eaf.auth.clearToken();
    this.storageService.deleteCookie(eaf.auth.tokenCookieName);
  }

  setToken(authToken: string, expireDate?: Date): void {
    this.storageService.setCookieValue(eaf.auth.tokenCookieName, authToken, expireDate, eaf.appPath, eaf.domain);
  }
}
