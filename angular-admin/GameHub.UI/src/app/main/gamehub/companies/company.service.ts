import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, OperatorFunction } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppConsts } from '@shared/AppConsts';

export interface CompanyDto {
  id: number;
  tenancyName: string;
  name: string;
  primaryContactEmail: string;
  country: string;
  isActive: boolean;
  creationTime: string;
  employeeCount: number;
}

export interface CreateOrUpdateCompanyInput {
  tenancyName: string;
  name: string;
  primaryContactEmail: string;
  country: string;
}

export interface CompanyEmployeeDto {
  userId: number;
  userName: string;
  emailAddress: string;
  role: string;
  isDefault: boolean;
  joinedAt?: string;
}

export interface InviteEmployeeInput {
  tenantId: number;
  emailOrUserName: string;
  role: string;
  isDefault?: boolean;
}

export interface RemoveEmployeeInput {
  tenantId: number;
  userId: number;
}

export interface SetDefaultEmployeeInput {
  tenantId: number;
  userId: number;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
}

function unwrapResult<T>(): OperatorFunction<any, T> {
  return map((response: any) => (response && response.result !== undefined ? response.result : response) as T);
}

@Injectable()
export class CompanyService {
  private readonly baseUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app`;

  constructor(private readonly http: HttpClient) {}

  getAll(skipCount = 0, maxResultCount = 50, sorting?: string): Observable<PagedResultDto<CompanyDto>> {
    const params = new HttpParams()
      .set('SkipCount', skipCount.toString())
      .set('MaxResultCount', maxResultCount.toString())
      .set('Sorting', sorting ?? '');

    return this.http.get(`${this.baseUrl}/Company/GetAll`, { params }).pipe(unwrapResult<PagedResultDto<CompanyDto>>());
  }

  get(id: number): Observable<CompanyDto> {
    return this.http.get(`${this.baseUrl}/Company/Get?id=${id}`).pipe(unwrapResult<CompanyDto>());
  }

  getByTenancyName(tenancyName: string): Observable<CompanyDto> {
    const params = new HttpParams().set('tenancyName', tenancyName);
    return this.http.get(`${this.baseUrl}/Company/GetByTenancyName`, { params }).pipe(unwrapResult<CompanyDto>());
  }

  create(input: CreateOrUpdateCompanyInput): Observable<CompanyDto> {
    return this.http.post(`${this.baseUrl}/Company/Create`, input).pipe(unwrapResult<CompanyDto>());
  }

  update(id: number, input: CreateOrUpdateCompanyInput): Observable<CompanyDto> {
    return this.http.put(`${this.baseUrl}/Company/Update?id=${id}`, input).pipe(unwrapResult<CompanyDto>());
  }

  delete(id: number): Observable<void> {
    return this.http.delete(`${this.baseUrl}/Company/Delete?id=${id}`).pipe(unwrapResult<void>());
  }

  getEmployees(tenantId: number): Observable<CompanyEmployeeDto[]> {
    return this.http.get(`${this.baseUrl}/CompanyEmployee/GetEmployees?tenantId=${tenantId}`).pipe(unwrapResult<CompanyEmployeeDto[]>());
  }

  invite(input: InviteEmployeeInput): Observable<CompanyEmployeeDto> {
    return this.http.post(`${this.baseUrl}/CompanyEmployee/Invite`, input).pipe(unwrapResult<CompanyEmployeeDto>());
  }

  remove(input: RemoveEmployeeInput): Observable<void> {
    return this.http.post(`${this.baseUrl}/CompanyEmployee/Remove`, input).pipe(unwrapResult<void>());
  }

  setDefault(input: SetDefaultEmployeeInput): Observable<void> {
    return this.http.post(`${this.baseUrl}/CompanyEmployee/SetDefault`, input).pipe(unwrapResult<void>());
  }
}
