///<reference path="../../../eaf-web-resources/Eaf/Framework/scripts/eaf.d.ts"/>

import { Injectable } from '@angular/core';
import { StorageService } from '@eaf/utils/storage.service';

export interface TokenPayload {
  sub?: string;
  unique_name?: string;
  name?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  nameidentifier?: string;
  role?: string | string[];
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  exp?: number;
  tenantid?: string;
}

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

  getPayload(token?: string): TokenPayload | null {
    const tokenValue = token ?? this.getToken();
    if (!tokenValue) {
      return null;
    }

    const parts = tokenValue.split('.');
    if (parts.length !== 3) {
      return null;
    }

    try {
      const payload = this.decodeBase64Url(parts[1]);
      return JSON.parse(payload) as TokenPayload;
    } catch {
      return null;
    }
  }

  isValid(): boolean {
    const payload = this.getPayload();
    if (!payload || !payload.exp) {
      return false;
    }
    return payload.exp * 1000 > Date.now();
  }

  getUserId(): number | null {
    const payload = this.getPayload();
    if (!payload) {
      return null;
    }

    const id =
      payload.sub ??
      payload.nameidentifier ??
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    if (!id) {
      return null;
    }

    const num = Number(id);
    return isNaN(num) ? null : num;
  }

  getTenantId(): number | null {
    const payload = this.getPayload();
    if (!payload || !payload.tenantid) {
      return null;
    }

    const num = Number(payload.tenantid);
    return isNaN(num) ? null : num;
  }

  getUserName(): string | null {
    const payload = this.getPayload();
    if (!payload) {
      return null;
    }

    return (
      payload.unique_name ??
      payload.name ??
      payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ??
      null
    );
  }

  getRoles(): string[] {
    const payload = this.getPayload();
    if (!payload) {
      return [];
    }

    const roles = payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (!roles) {
      return [];
    }

    return Array.isArray(roles) ? roles : [roles];
  }

  isInRole(role: string): boolean {
    return this.getRoles().includes(role);
  }

  private decodeBase64Url(value: string): string {
    const base64 = value
      .replace(/-/g, '+')
      .replace(/_/g, '/')
      .padEnd(value.length + ((4 - (value.length % 4)) % 4), '=');

    const binary = atob(base64);
    return decodeURIComponent(escape(binary));
  }
}
