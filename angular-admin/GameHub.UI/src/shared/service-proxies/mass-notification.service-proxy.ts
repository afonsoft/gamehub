import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, of as _observableOf, throwError as _observableThrow } from 'rxjs';
import { catchError as _observableCatch, mergeMap as _observableMergeMap } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface ICreateMassNotificationInput {
    subject: string;
    message: string;
    severity?: number;
    targetUserIds?: string;
    targetRoleIds?: string;
    targetOrganizationUnitIds?: string;
    sendToAllUsers?: boolean;
    scheduledTime?: Date | string;
}

export interface IMassNotificationDto {
    id: number;
    tenantId?: number;
    subject: string;
    message: string;
    severity: number;
    targetUserIds?: string;
    targetRoleIds?: string;
    targetOrganizationUnitIds?: string;
    sendToAllUsers: boolean;
    status: string;
    scheduledTime?: Date | string;
}

export interface IGetMassNotificationsInput {
    filter?: string;
    status?: string;
    sorting?: string;
    skipCount?: number;
    maxResultCount?: number;
}

export interface IPagedResultDtoOfMassNotificationDto {
    totalCount: number;
    items: IMassNotificationDto[];
}

@Injectable()
export class MassNotificationServiceProxy {
    private readonly http: HttpClient;
    private readonly baseUrl: string;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ?? '';
    }

    getAll(input: IGetMassNotificationsInput): Observable<IPagedResultDtoOfMassNotificationDto> {
        let url_ = this.baseUrl + '/api/services/app/MassNotification/GetAll?';
        if (input.filter !== undefined && input.filter !== null) url_ += 'Filter=' + encodeURIComponent('' + input.filter) + '&';
        if (input.status !== undefined && input.status !== null) url_ += 'Status=' + encodeURIComponent('' + input.status) + '&';
        if (input.sorting !== undefined && input.sorting !== null) url_ += 'Sorting=' + encodeURIComponent('' + input.sorting) + '&';
        if (input.skipCount !== undefined && input.skipCount !== null) url_ += 'SkipCount=' + encodeURIComponent('' + input.skipCount) + '&';
        if (input.maxResultCount !== undefined && input.maxResultCount !== null) url_ += 'MaxResultCount=' + encodeURIComponent('' + input.maxResultCount) + '&';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processPaged(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    create(input: ICreateMassNotificationInput): Observable<IMassNotificationDto> {
        const url_ = this.baseUrl + '/api/services/app/MassNotification/Create';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processMassNotification(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    cancel(id: number): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/MassNotification/Cancel';
        const options: unknown = { observe: 'response', responseType: 'json', body: { id } };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processVoid(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processPaged(response: HttpResponse<any>): Observable<IPagedResultDtoOfMassNotificationDto> {
        const status = response.status;
        const responseBlob = this.unwrapResult(response.body);
        if (status === 200) {
            return _observableOf(responseBlob as IPagedResultDtoOfMassNotificationDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    private processMassNotification(response: HttpResponse<any>): Observable<IMassNotificationDto> {
        const status = response.status;
        const responseBlob = this.unwrapResult(response.body);
        if (status === 200) {
            return _observableOf(responseBlob as IMassNotificationDto);
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
    private unwrapResult(value: any): any {
        return value?.result ?? value;
    }
}
