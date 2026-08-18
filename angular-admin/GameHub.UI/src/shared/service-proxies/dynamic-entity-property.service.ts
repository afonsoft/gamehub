import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';

export class DynamicPropertyValueDto {
  id: number | undefined;
  dynamicPropertyId: number | undefined;
  value: string | undefined;
  tenantId: number | undefined;
}

export class DynamicPropertyDto {
  id: number | undefined;
  propertyName: string | undefined;
  displayName: string | undefined;
  inputType: string | undefined;
  permission: string | undefined;
  tenantId: number | undefined;
  values: DynamicPropertyValueDto[] | undefined;
}

export class CreateOrUpdateDynamicPropertyInput {
  id: number | undefined;
  propertyName: string | undefined;
  displayName: string | undefined;
  inputType: string | undefined;
  permission: string | undefined;
  values: DynamicPropertyValueDto[] | undefined;
}

export class DynamicEntityPropertyDto {
  id: number | undefined;
  entityFullName: string | undefined;
  dynamicPropertyId: number | undefined;
  tenantId: number | undefined;
  dynamicProperty: DynamicPropertyDto | undefined;
}

export class CreateDynamicEntityPropertyInput {
  entityFullName: string | undefined;
  dynamicPropertyId: number | undefined;
}

export class DynamicEntityPropertyValueDto {
  id: number | undefined;
  entityId: string | undefined;
  dynamicEntityPropertyId: number | undefined;
  value: string | undefined;
  tenantId: number | undefined;
  dynamicEntityProperty: DynamicEntityPropertyDto | undefined;
}

export class CreateOrUpdateDynamicEntityPropertyValueInput {
  id: number | undefined;
  entityId: string | undefined;
  dynamicEntityPropertyId: number | undefined;
  value: string | undefined;
}

export class GetDynamicEntityPropertyValuesInput {
  entityFullName: string | undefined;
  entityId: string | undefined;
  dynamicEntityPropertyId: number | undefined;
  dynamicPropertyId: number | undefined;
  propertyName: string | undefined;
}

export class ListResultDto<T> {
  items: T[] | undefined;
}

@Injectable()
export class DynamicEntityPropertyService extends AppComponentBase {
  private readonly _dynamicPropertyUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/DynamicPropertyAppService`;
  private readonly _dynamicEntityPropertyUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/DynamicEntityPropertyAppService`;
  private readonly _dynamicEntityPropertyValueUrl = `${AppConsts.remoteServiceBaseUrl}/api/services/app/DynamicEntityPropertyValueAppService`;

  constructor(
    injector: Injector,
    private readonly _httpClient: HttpClient,
  ) {
    super(injector);
  }

  getAllDynamicProperties(): Observable<ListResultDto<DynamicPropertyDto>> {
    return this._httpClient.get<ListResultDto<DynamicPropertyDto>>(`${this._dynamicPropertyUrl}/GetAll`);
  }

  getDynamicProperty(id: number): Observable<DynamicPropertyDto> {
    return this._httpClient.get<DynamicPropertyDto>(`${this._dynamicPropertyUrl}/Get`, { params: new HttpParams().set('id', id.toString()) });
  }

  createDynamicProperty(input: CreateOrUpdateDynamicPropertyInput): Observable<DynamicPropertyDto> {
    return this._httpClient.post<DynamicPropertyDto>(`${this._dynamicPropertyUrl}/Create`, input);
  }

  updateDynamicProperty(input: CreateOrUpdateDynamicPropertyInput): Observable<DynamicPropertyDto> {
    return this._httpClient.post<DynamicPropertyDto>(`${this._dynamicPropertyUrl}/Update`, input);
  }

  deleteDynamicProperty(id: number): Observable<void> {
    return this._httpClient.delete<void>(`${this._dynamicPropertyUrl}/Delete`, { params: new HttpParams().set('id', id.toString()) });
  }

  getAllDynamicEntityProperties(entityFullName?: string): Observable<ListResultDto<DynamicEntityPropertyDto>> {
    let params = new HttpParams();
    if (entityFullName) {
      params = params.set('entityFullName', entityFullName);
    }
    return this._httpClient.get<ListResultDto<DynamicEntityPropertyDto>>(`${this._dynamicEntityPropertyUrl}/GetAll`, { params });
  }

  createDynamicEntityProperty(input: CreateDynamicEntityPropertyInput): Observable<DynamicEntityPropertyDto> {
    return this._httpClient.post<DynamicEntityPropertyDto>(`${this._dynamicEntityPropertyUrl}/Create`, input);
  }

  deleteDynamicEntityProperty(id: number): Observable<void> {
    return this._httpClient.delete<void>(`${this._dynamicEntityPropertyUrl}/Delete`, { params: new HttpParams().set('id', id.toString()) });
  }

  getAllDynamicEntityPropertyValues(input: GetDynamicEntityPropertyValuesInput): Observable<ListResultDto<DynamicEntityPropertyValueDto>> {
    let params = new HttpParams();
    if (input.entityFullName) {
      params = params.set('entityFullName', input.entityFullName);
    }
    if (input.entityId) {
      params = params.set('entityId', input.entityId);
    }
    if (input.dynamicEntityPropertyId !== undefined && input.dynamicEntityPropertyId !== null) {
      params = params.set('dynamicEntityPropertyId', input.dynamicEntityPropertyId.toString());
    }
    if (input.dynamicPropertyId !== undefined && input.dynamicPropertyId !== null) {
      params = params.set('dynamicPropertyId', input.dynamicPropertyId.toString());
    }
    if (input.propertyName) {
      params = params.set('propertyName', input.propertyName);
    }
    return this._httpClient.get<ListResultDto<DynamicEntityPropertyValueDto>>(`${this._dynamicEntityPropertyValueUrl}/GetAll`, { params });
  }

  createDynamicEntityPropertyValue(input: CreateOrUpdateDynamicEntityPropertyValueInput): Observable<DynamicEntityPropertyValueDto> {
    return this._httpClient.post<DynamicEntityPropertyValueDto>(`${this._dynamicEntityPropertyValueUrl}/Create`, input);
  }

  updateDynamicEntityPropertyValue(input: CreateOrUpdateDynamicEntityPropertyValueInput): Observable<DynamicEntityPropertyValueDto> {
    return this._httpClient.post<DynamicEntityPropertyValueDto>(`${this._dynamicEntityPropertyValueUrl}/Update`, input);
  }

  deleteDynamicEntityPropertyValue(id: number): Observable<void> {
    return this._httpClient.delete<void>(`${this._dynamicEntityPropertyValueUrl}/Delete`, { params: new HttpParams().set('id', id.toString()) });
  }
}
