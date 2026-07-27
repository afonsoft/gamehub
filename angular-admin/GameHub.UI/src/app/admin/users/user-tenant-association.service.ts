import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';

export interface UserTenantMembershipDto {
  tenantId: number;
  tenantName?: string;
  tenantTenancyName?: string;
  isDefault: boolean;
}

export interface AssociateUserToTenantInput {
  userId: number;
  tenantId: number;
  isDefault?: boolean;
}

export interface RemoveUserTenantAssociationInput {
  userId: number;
  tenantId: number;
}

export interface SetDefaultTenantInput {
  userId: number;
  tenantId: number;
}

@Injectable()
export class UserTenantAssociationService {
  private readonly baseUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/UserTenantAssociation`;

  constructor(private readonly http: HttpClient) {}

  getByUser(userId: number): Observable<UserTenantMembershipDto[]> {
    return this.http.post<UserTenantMembershipDto[]>(`${this.baseUrl}/GetUserMemberships`, { userId });
  }

  associate(input: AssociateUserToTenantInput): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/Associate`, input);
  }

  remove(input: RemoveUserTenantAssociationInput): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/RemoveAssociation`, input);
  }

  setDefault(input: SetDefaultTenantInput): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/SetDefault`, input);
  }
}
