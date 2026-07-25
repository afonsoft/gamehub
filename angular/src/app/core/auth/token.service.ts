import { Injectable } from '@angular/core';

export interface TokenPayload {
  sub?: string;
  unique_name?: string;
  name?: string;
  nameidentifier?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  userId?: string;
  role?: string | string[];
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
  exp?: number;
}

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly tokenKey = 'gamehub_token';

  getToken(): string | null {
    try {
      return typeof window !== 'undefined' ? localStorage.getItem(this.tokenKey) : null;
    } catch {
      return null;
    }
  }

  setToken(token: string): void {
    try {
      if (typeof window !== 'undefined') {
        localStorage.setItem(this.tokenKey, token);
      }
    } catch {
      // Ignore in private/incognito mode.
    }
  }

  clearToken(): void {
    try {
      if (typeof window !== 'undefined') {
        localStorage.removeItem(this.tokenKey);
      }
    } catch {
      // Ignore in private/incognito mode.
    }
  }

  isValid(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }
    try {
      const payload = this.getPayload(token);
      if (!payload?.exp) {
        return true;
      }
      return payload.exp * 1000 > Date.now();
    } catch {
      return false;
    }
  }

  getPayload(token?: string): TokenPayload | null {
    const t = token ?? this.getToken();
    if (!t) {
      return null;
    }
    try {
      const base64 = t.split('.')[1]?.replace(/-/g, '+').replace(/_/g, '/');
      if (!base64) {
        return null;
      }
      const json = atob(base64);
      return JSON.parse(json) as TokenPayload;
    } catch {
      return null;
    }
  }

  getUserId(): number | null {
    const payload = this.getPayload();
    const raw =
      payload?.userId ??
      payload?.nameidentifier ??
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
      payload?.sub;
    if (!raw) {
      return null;
    }
    const parsed = Number(raw);
    return Number.isNaN(parsed) ? null : parsed;
  }

  getUserName(): string | null {
    const payload = this.getPayload();
    return (
      payload?.unique_name ??
      payload?.name ??
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ??
      null
    );
  }

  getRoles(): string[] {
    const payload = this.getPayload();
    const role = payload?.role ?? payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (!role) {
      return [];
    }
    return Array.isArray(role) ? role : [role];
  }

  isInRole(role: string): boolean {
    return this.getRoles().map(r => r.toLowerCase()).includes(role.toLowerCase());
  }
}
