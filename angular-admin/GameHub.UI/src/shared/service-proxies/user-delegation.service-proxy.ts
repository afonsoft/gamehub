import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, of as _observableOf, throwError as _observableThrow } from 'rxjs';
import { catchError as _observableCatch, mergeMap as _observableMergeMap } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface ICreateUserDelegationInput {
    targetUserId: number;
    startTime: Date | string;
    endTime: Date | string;
    description?: string;
}

export interface IUserDelegationDto {
    id: number;
    tenantId?: number;
    sourceUserId: number;
    targetUserId: number;
    sourceUserName: string;
    targetUserName: string;
    startTime: Date | string;
    endTime: Date | string;
    description: string;
    isActive: boolean;
}

export interface IGetUserDelegationsInput {
    sourceUserId?: number;
    targetUserId?: number;
    sorting?: string;
    skipCount?: number;
    maxResultCount?: number;
}

export interface IListResultDtoOfUserDelegationDto {
    items: IUserDelegationDto[];
}

@Injectable()
export class UserDelegationServiceProxy {
    private readonly http: HttpClient;
    private readonly baseUrl: string;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ?? '';
    }

    getMyDelegations(input: IGetUserDelegationsInput): Observable<IListResultDtoOfUserDelegationDto> {
        let url_ = this.baseUrl + '/api/services/app/UserDelegationAppService/GetMyDelegations?';
        if (input.sourceUserId !== undefined && input.sourceUserId !== null) url_ += 'SourceUserId=' + encodeURIComponent('' + input.sourceUserId) + '&';
        if (input.targetUserId !== undefined && input.targetUserId !== null) url_ += 'TargetUserId=' + encodeURIComponent('' + input.targetUserId) + '&';
        if (input.sorting !== undefined && input.sorting !== null) url_ += 'Sorting=' + encodeURIComponent('' + input.sorting) + '&';
        if (input.skipCount !== undefined && input.skipCount !== null) url_ += 'SkipCount=' + encodeURIComponent('' + input.skipCount) + '&';
        if (input.maxResultCount !== undefined && input.maxResultCount !== null) url_ += 'MaxResultCount=' + encodeURIComponent('' + input.maxResultCount) + '&';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processList(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    getDelegatedUsers(input: IGetUserDelegationsInput): Observable<IListResultDtoOfUserDelegationDto> {
        let url_ = this.baseUrl + '/api/services/app/UserDelegationAppService/GetDelegatedUsers?';
        if (input.sourceUserId !== undefined && input.sourceUserId !== null) url_ += 'SourceUserId=' + encodeURIComponent('' + input.sourceUserId) + '&';
        if (input.targetUserId !== undefined && input.targetUserId !== null) url_ += 'TargetUserId=' + encodeURIComponent('' + input.targetUserId) + '&';
        if (input.sorting !== undefined && input.sorting !== null) url_ += 'Sorting=' + encodeURIComponent('' + input.sorting) + '&';
        if (input.skipCount !== undefined && input.skipCount !== null) url_ += 'SkipCount=' + encodeURIComponent('' + input.skipCount) + '&';
        if (input.maxResultCount !== undefined && input.maxResultCount !== null) url_ += 'MaxResultCount=' + encodeURIComponent('' + input.maxResultCount) + '&';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processList(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    create(input: ICreateUserDelegationInput): Observable<IUserDelegationDto> {
        const url_ = this.baseUrl + '/api/services/app/UserDelegationAppService/Create';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processUserDelegation(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    cancel(id: number): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/UserDelegationAppService/Cancel';
        const options: unknown = { observe: 'response', responseType: 'json', body: { id } };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processList(response: HttpResponse<any>): Observable<IListResultDtoOfUserDelegationDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IListResultDtoOfUserDelegationDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    private processUserDelegation(response: HttpResponse<any>): Observable<IUserDelegationDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IUserDelegationDto);
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
