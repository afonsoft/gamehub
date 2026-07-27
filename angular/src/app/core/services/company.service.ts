import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface PublicCompanyDto {
  id: number;
  tenancyName: string;
  name: string;
  primaryContactEmail: string;
  country: string;
  employeeCount: number;
  creationTime: string;
}

export interface CompanyEmployeeDto {
  userId: number;
  userName: string;
  emailAddress: string;
  role: string;
  isDefault: boolean;
  joinedAt?: string;
}

export interface RegisterAsCompanyEmployeeInput {
  tenancyName: string;
  name: string;
  surname: string;
  userName: string;
  emailAddress: string;
  password: string;
  role: string;
}

export interface CompanyEmployeeDto {
  userId: number;
  userName: string;
  emailAddress: string;
  role: string;
  isDefault: boolean;
  joinedAt?: string;
}

function unwrap<T>(response: T | { result?: T }): T {
  if (response && typeof response === 'object' && 'result' in response) {
    return (response as { result?: T }).result as T;
  }
  return response as T;
}

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/services/app/Company';

  getByTenancyName(tenancyName: string): Observable<PublicCompanyDto> {
    const params = new HttpParams().set('tenancyName', tenancyName);
    return this.http.get<PublicCompanyDto | { result?: PublicCompanyDto }>(`${this.baseUrl}/GetByTenancyName`, { params })
      .pipe(map(response => unwrap(response)));
  }

  registerAndJoin(input: RegisterAsCompanyEmployeeInput): Observable<CompanyEmployeeDto> {
    return this.http.post<CompanyEmployeeDto | { result?: CompanyEmployeeDto }>(`${this.baseUrl}Employee/RegisterAndJoin`, input)
      .pipe(map(response => unwrap(response)));
  }
}
