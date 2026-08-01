import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, of as _observableOf, throwError as _observableThrow } from 'rxjs';
import { catchError as _observableCatch, mergeMap as _observableMergeMap } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface ITenantSubscriptionDto {
    tenantId: number;
    editionId?: number;
    editionDisplayName?: string;
    subscriptionEndDateUtc?: Date | string;
    remainingDays?: number;
    isActive: boolean;
}

export interface IAssignEditionToTenantInput {
    tenantId: number;
    editionId: number;
    paymentPeriodType: number;
}

export interface IExtendTenantSubscriptionInput {
    tenantId: number;
    paymentPeriodType: number;
}

@Injectable()
export class TenantSubscriptionServiceProxy {
    private readonly http: HttpClient;
    private readonly baseUrl: string;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ?? '';
    }

    getTenantSubscription(tenantId: number): Observable<ITenantSubscriptionDto> {
        const url_ = this.baseUrl + '/api/services/app/Tenant/GetTenantSubscription?Id=' + encodeURIComponent('' + tenantId);
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => {
            const status = response.status;
            const body = response.body ?? {};
            if (status === 200) {
                return _observableOf(body as ITenantSubscriptionDto);
            }
            return _observableThrow(new Error('Unexpected response: ' + status));
        })).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    assignEditionToTenant(input: IAssignEditionToTenantInput): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/Tenant/AssignEditionToTenant';
        const options: unknown = { body: input, headers: { 'Content-Type': 'application/json' }, observe: 'response', responseType: 'blob' };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processAction(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    extendTenantSubscription(input: IExtendTenantSubscriptionInput): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/Tenant/ExtendTenantSubscription';
        const options: unknown = { body: input, headers: { 'Content-Type': 'application/json' }, observe: 'response', responseType: 'blob' };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processAction(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processAction(response: HttpResponse<any>): Observable<void> {
        const status = response.status;
        if (status === 200) {
            return _observableOf(undefined as any);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }
}
