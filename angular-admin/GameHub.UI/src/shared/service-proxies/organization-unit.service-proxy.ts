import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, of as _observableOf, throwError as _observableThrow } from 'rxjs';
import { catchError as _observableCatch, mergeMap as _observableMergeMap } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface ICreateOrganizationUnitInput {
    displayName: string;
    parentId?: number;
}

export interface IUpdateOrganizationUnitInput {
    id: number;
    displayName: string;
}

export interface IMoveOrganizationUnitInput {
    id: number;
    newParentId?: number;
}

export interface IOrganizationUnitDto {
    id: number;
    displayName: string;
    code: string;
    parentId?: number;
    children?: IOrganizationUnitDto[];
}

export interface IGetOrganizationUnitUsersInput {
    organizationUnitId: number;
    filter?: string;
    sorting?: string;
    skipCount?: number;
    maxResultCount?: number;
}

export interface IOrganizationUnitUserListDto {
    userId: number;
    userName: string;
    name: string;
    surname: string;
    emailAddress: string;
}

export interface IOrganizationUnitRoleListDto {
    roleId: number;
    roleName: string;
    roleDisplayName: string;
}

export interface IUserToOrganizationUnitInput {
    organizationUnitId: number;
    userId: number;
}

export interface IRoleToOrganizationUnitInput {
    organizationUnitId: number;
    roleId: number;
}

export interface IPagedResultDtoOfOrganizationUnitUserListDto {
    totalCount: number;
    items: IOrganizationUnitUserListDto[];
}

export interface IPagedResultDtoOfOrganizationUnitRoleListDto {
    totalCount: number;
    items: IOrganizationUnitRoleListDto[];
}

@Injectable()
export class OrganizationUnitServiceProxy {
    private readonly http: HttpClient;
    private readonly baseUrl: string;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ?? '';
    }

    getOrganizationUnits(): Observable<IOrganizationUnitDto[]> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/GetOrganizationUnits';
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processOrganizationUnits(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processOrganizationUnits(response: HttpResponse<any>): Observable<IOrganizationUnitDto[]> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf((responseBlob as any).items as IOrganizationUnitDto[]);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    create(input: ICreateOrganizationUnitInput): Observable<IOrganizationUnitDto> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/Create';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processOrganizationUnit(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    update(input: IUpdateOrganizationUnitInput): Observable<IOrganizationUnitDto> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/Update';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('put', url_, options).pipe(_observableMergeMap((response: any) => this.processOrganizationUnit(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    move(input: IMoveOrganizationUnitInput): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/Move';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    delete(id: number): Observable<void> {
        let url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/Delete?Id=' + encodeURIComponent('' + id) + '&';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('delete', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    getOrganizationUnitUsers(input: IGetOrganizationUnitUsersInput): Observable<IPagedResultDtoOfOrganizationUnitUserListDto> {
        let url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/GetOrganizationUnitUsers?OrganizationUnitId=' + encodeURIComponent('' + input.organizationUnitId) + '&';
        if (input.filter !== undefined && input.filter !== null) url_ += 'Filter=' + encodeURIComponent('' + input.filter) + '&';
        if (input.sorting !== undefined && input.sorting !== null) url_ += 'Sorting=' + encodeURIComponent('' + input.sorting) + '&';
        if (input.skipCount !== undefined && input.skipCount !== null) url_ += 'SkipCount=' + encodeURIComponent('' + input.skipCount) + '&';
        if (input.maxResultCount !== undefined && input.maxResultCount !== null) url_ += 'MaxResultCount=' + encodeURIComponent('' + input.maxResultCount) + '&';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processPagedUsers(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processPagedUsers(response: HttpResponse<any>): Observable<IPagedResultDtoOfOrganizationUnitUserListDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IPagedResultDtoOfOrganizationUnitUserListDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    getOrganizationUnitRoles(input: IGetOrganizationUnitUsersInput): Observable<IPagedResultDtoOfOrganizationUnitRoleListDto> {
        let url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/GetOrganizationUnitRoles?OrganizationUnitId=' + encodeURIComponent('' + input.organizationUnitId) + '&';
        if (input.filter !== undefined && input.filter !== null) url_ += 'Filter=' + encodeURIComponent('' + input.filter) + '&';
        if (input.sorting !== undefined && input.sorting !== null) url_ += 'Sorting=' + encodeURIComponent('' + input.sorting) + '&';
        if (input.skipCount !== undefined && input.skipCount !== null) url_ += 'SkipCount=' + encodeURIComponent('' + input.skipCount) + '&';
        if (input.maxResultCount !== undefined && input.maxResultCount !== null) url_ += 'MaxResultCount=' + encodeURIComponent('' + input.maxResultCount) + '&';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processPagedRoles(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processPagedRoles(response: HttpResponse<any>): Observable<IPagedResultDtoOfOrganizationUnitRoleListDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IPagedResultDtoOfOrganizationUnitRoleListDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    addUserToOrganizationUnit(input: IUserToOrganizationUnitInput): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/AddUserToOrganizationUnit';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    removeUserFromOrganizationUnit(input: IUserToOrganizationUnitInput): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/RemoveUserFromOrganizationUnit';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    addRoleToOrganizationUnit(input: IRoleToOrganizationUnitInput): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/AddRoleToOrganizationUnit';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    removeRoleFromOrganizationUnit(input: IRoleToOrganizationUnitInput): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/OrganizationUnitAppService/RemoveRoleFromOrganizationUnit';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processOrganizationUnit(response: HttpResponse<any>): Observable<IOrganizationUnitDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IOrganizationUnitDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    private processVoid(response: HttpResponse<any>): Observable<void> {
        const status = response.status;
        if (status === 200 || status === 204) {
            return _observableOf(undefined as any);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }
}
