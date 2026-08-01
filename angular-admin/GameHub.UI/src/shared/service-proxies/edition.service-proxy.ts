import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, of as _observableOf, throwError as _observableThrow } from 'rxjs';
import { catchError as _observableCatch, mergeMap as _observableMergeMap } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface INameValueDto {
    name: string;
    value: string;
}

export interface ICreateEditionInput {
    displayName: string;
    isFree: boolean;
    monthlyPrice?: number;
    annualPrice?: number;
    quarterlyPrice?: number;
    biannualPrice?: number;
    permanentPrice?: number;
    defaultPaymentPeriodType?: number;
    trialDayCount?: number;
    waitingDayAfterExpire?: number;
    expiringEditionId?: number;
}

export interface IUpdateEditionInput extends ICreateEditionInput {
    id: number;
}

export interface IEditionDto {
    displayName: string;
    isFree: boolean;
    monthlyPrice?: number;
    annualPrice?: number;
    quarterlyPrice?: number;
    biannualPrice?: number;
    permanentPrice?: number;
    defaultPaymentPeriodType?: number;
    trialDayCount?: number;
    waitingDayAfterExpire?: number;
    expiringEditionId?: number;
    id: number;
}

export interface IPagedResultDtoOfEditionDto {
    totalCount: number;
    items: IEditionDto[];
}

export interface IFlatFeatureDto {
    name: string;
    displayName: string;
    description?: string;
    defaultValue: string;
    inputType: { name: string; attributes: any; validator: any };
    parentName?: string;
}

export interface IGetEditionFeaturesEditOutput {
    features: IFlatFeatureDto[];
    featureValues: INameValueDto[];
}

export interface IUpdateEditionFeaturesInput {
    id: number;
    featureValues: INameValueDto[];
}

@Injectable()
export class EditionServiceProxy {
    private readonly http: HttpClient;
    private readonly baseUrl: string;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ?? '';
    }

    getEditions(filter: string | undefined, sorting: string | undefined, maxResultCount: number | undefined, skipCount: number | undefined): Observable<IPagedResultDtoOfEditionDto> {
        let url_ = this.baseUrl + '/api/services/app/Edition/GetEditions?';
        if (filter !== undefined && filter !== null) url_ += 'Filter=' + encodeURIComponent('' + filter) + '&';
        if (sorting !== undefined && sorting !== null) url_ += 'Sorting=' + encodeURIComponent('' + sorting) + '&';
        if (skipCount !== undefined && skipCount !== null) url_ += 'SkipCount=' + encodeURIComponent('' + skipCount) + '&';
        if (maxResultCount !== undefined && maxResultCount !== null) url_ += 'MaxResultCount=' + encodeURIComponent('' + maxResultCount) + '&';
        url_ = url_.replace(/[?&]$/, '');

        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processGetEditions(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processGetEditions(response: HttpResponse<any>): Observable<IPagedResultDtoOfEditionDto> {
        const status = response.status;
        const responseBlob = this.unwrapResult(response.body);
        if (status === 200) {
            return _observableOf(responseBlob as IPagedResultDtoOfEditionDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    getEditionForEdit(id: number): Observable<IEditionDto> {
        let url_ = this.baseUrl + '/api/services/app/Edition/GetEditionForEdit?';
        url_ += 'Id=' + encodeURIComponent('' + id) + '&';
        url_ = url_.replace(/[?&]$/, '');

        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processEdition(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processEdition(response: HttpResponse<any>): Observable<IEditionDto> {
        const status = response.status;
        const responseBlob = this.unwrapResult(response.body);
        if (status === 200) {
            return _observableOf(responseBlob as IEditionDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    createEdition(input: ICreateEditionInput): Observable<void> {
        let url_ = this.baseUrl + '/api/services/app/Edition/CreateEdition';
        url_ = url_.replace(/[?&]$/, '');

        const content_ = JSON.stringify(input);
        const options: unknown = { body: content_, headers: { 'Content-Type': 'application/json' }, observe: 'response', responseType: 'blob' };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processAction(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    updateEdition(input: IUpdateEditionInput): Observable<void> {
        let url_ = this.baseUrl + '/api/services/app/Edition/UpdateEdition';
        url_ = url_.replace(/[?&]$/, '');

        const content_ = JSON.stringify(input);
        const options: unknown = { body: content_, headers: { 'Content-Type': 'application/json' }, observe: 'response', responseType: 'blob' };
        return this.http.request('put', url_, options).pipe(_observableMergeMap((response: any) => this.processAction(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    deleteEdition(id: number): Observable<void> {
        let url_ = this.baseUrl + '/api/services/app/Edition/DeleteEdition?';
        url_ += 'Id=' + encodeURIComponent('' + id) + '&';
        url_ = url_.replace(/[?&]$/, '');

        const options: unknown = { observe: 'response', responseType: 'blob' };
        return this.http.request('delete', url_, options).pipe(_observableMergeMap((response: any) => this.processAction(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    getEditionFeaturesForEdit(id: number): Observable<IGetEditionFeaturesEditOutput> {
        let url_ = this.baseUrl + '/api/services/app/Edition/GetEditionFeaturesForEdit?';
        url_ += 'Id=' + encodeURIComponent('' + id) + '&';
        url_ = url_.replace(/[?&]$/, '');

        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processFeatures(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processFeatures(response: HttpResponse<any>): Observable<IGetEditionFeaturesEditOutput> {
        const status = response.status;
        const responseBlob = this.unwrapResult(response.body);
        if (status === 200) {
            return _observableOf(responseBlob as IGetEditionFeaturesEditOutput);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    updateEditionFeatures(input: IUpdateEditionFeaturesInput): Observable<void> {
        let url_ = this.baseUrl + '/api/services/app/Edition/UpdateEditionFeatures';
        url_ = url_.replace(/[?&]$/, '');

        const content_ = JSON.stringify(input);
        const options: unknown = { body: content_, headers: { 'Content-Type': 'application/json' }, observe: 'response', responseType: 'blob' };
        return this.http.request('put', url_, options).pipe(_observableMergeMap((response: any) => this.processAction(response))).pipe(_observableCatch((response: any) => {
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
    private unwrapResult(value: any): any {
        return value?.result ?? value;
    }
}
