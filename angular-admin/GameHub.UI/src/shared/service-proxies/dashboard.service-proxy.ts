import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, of as _observableOf, throwError as _observableThrow } from 'rxjs';
import { catchError as _observableCatch, mergeMap as _observableMergeMap } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface IDashboardTileDto {
    id: string;
    title: string;
    count: number;
    style: string;
    icon: string;
}

export interface IDashboardOutput {
    tiles: IDashboardTileDto[];
    isHostDashboard: boolean;
}

@Injectable()
export class DashboardServiceProxy {
    private readonly http: HttpClient;
    private readonly baseUrl: string;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ?? '';
    }

    getHostDashboard(): Observable<IDashboardOutput> {
        let url_ = this.baseUrl + '/api/services/app/Dashboard/GetHostDashboard';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processDashboard(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    getTenantDashboard(): Observable<IDashboardOutput> {
        let url_ = this.baseUrl + '/api/services/app/Dashboard/GetTenantDashboard';
        url_ = url_.replace(/[?&]$/, '');
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => this.processDashboard(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processDashboard(response: HttpResponse<any>): Observable<IDashboardOutput> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IDashboardOutput);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }
}
