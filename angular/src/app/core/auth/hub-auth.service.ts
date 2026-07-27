import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface AvailableTenantResult {
  tenantId: number;
  tenantName?: string;
  tenancyName?: string;
  isDefault: boolean;
}

export interface SelectTenantModel {
  userNameOrEmailAddress: string;
  password: string;
  tenantId: number;
}

export interface SelectTenantResult {
  accessToken: string;
  expireInSeconds: number;
  userId: number;
  tenantId: number;
}

@Injectable({ providedIn: 'root' })
export class HubAuthService {
  private readonly http = inject(HttpClient);
  private readonly availableTenantsUrl = '/api/hub/auth/available-tenants';
  private readonly selectTenantUrl = '/api/hub/auth/select-tenant';

  getAvailableTenants(model: { userNameOrEmailAddress: string; password: string }): Observable<AvailableTenantResult[]> {
    return this.http.post<AvailableTenantResult[] | { result?: AvailableTenantResult[] }>(this.availableTenantsUrl, model)
      .pipe(map(response => this.unwrap(response)));
  }

  selectTenant(model: SelectTenantModel): Observable<SelectTenantResult> {
    return this.http
      .post<SelectTenantResult | { result?: SelectTenantResult }>(this.selectTenantUrl, model)
      .pipe(map(response => this.unwrap(response)));
  }

  private unwrap<T>(response: T | { result?: T }): T {
    if (response && typeof response === 'object' && 'result' in response) {
      return (response as { result?: T }).result as T;
    }
    return response as T;
  }
}
