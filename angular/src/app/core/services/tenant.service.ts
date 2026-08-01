import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface AvailableTenant {
  id: number;
  name: string;
  tenancyName: string;
  isActive: boolean;
}

export interface TenantJoinRequest {
  id: number;
  tenantId: number;
  tenantName: string;
  userId: number;
  userName: string;
  userFullName: string;
  status: string;
  message?: string;
  creationTime: string;
}

export interface CreateJoinRequestInput {
  tenantId: number;
  message?: string;
}

function unwrap<T>(response: T | { result?: T }): T {
  if (response && typeof response === 'object' && 'result' in response) {
    return (response as { result?: T }).result as T;
  }
  return response as T;
}

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/services/app/TenantJoinRequest';

  getAvailableTenants(): Observable<AvailableTenant[]> {
    return this.http
      .get<AvailableTenant[] | { result?: AvailableTenant[] }>(`${this.baseUrl}/GetAvailableTenants`, {})
      .pipe(map(response => unwrap(response)));
  }

  getMyRequests(): Observable<TenantJoinRequest[]> {
    return this.http
      .get<TenantJoinRequest[] | { result?: TenantJoinRequest[] }>(`${this.baseUrl}/GetMyRequests`)
      .pipe(map(response => unwrap(response)));
  }

  createRequest(input: CreateJoinRequestInput): Observable<TenantJoinRequest> {
    return this.http
      .post<TenantJoinRequest | { result?: TenantJoinRequest }>(`${this.baseUrl}/CreateRequest`, input)
      .pipe(map(response => unwrap(response)));
  }

  getPendingRequests(): Observable<TenantJoinRequest[]> {
    return this.http
      .get<TenantJoinRequest[] | { result?: TenantJoinRequest[] }>(`${this.baseUrl}/GetPendingRequestsForCurrentTenant`)
      .pipe(map(response => unwrap(response)));
  }

  approveRequest(requestId: number, approved: boolean): Observable<TenantJoinRequest> {
    return this.http
      .post<TenantJoinRequest | { result?: TenantJoinRequest }>(`${this.baseUrl}/Approve`, { requestId, approved })
      .pipe(map(response => unwrap(response)));
  }
}
