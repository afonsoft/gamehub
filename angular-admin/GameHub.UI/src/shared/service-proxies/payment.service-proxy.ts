import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, of as _observableOf, throwError as _observableThrow } from 'rxjs';
import { catchError as _observableCatch, mergeMap as _observableMergeMap } from 'rxjs/operators';
import { API_BASE_URL } from '@shared/service-proxies/service-proxies';

export interface ICreateSubscriptionPaymentInput {
    editionId: number;
    editionPaymentType: number;
    paymentPeriodType: number;
    gateway?: string;
    description?: string;
}

export interface IProcessPaymentInput {
    externalPaymentId?: string;
    gateway?: string;
    gatewayResponse?: string;
    isSuccess: boolean;
}

export interface IPaymentRequestDto {
    paymentId: string;
    gateway: string;
    checkoutUrl?: string;
    isSuccess: boolean;
}

export interface ISubscriptionPaymentDto {
    id: number;
    tenantId?: number;
    editionId: number;
    editionPaymentType: number;
    paymentPeriodType: number;
    amount: number;
    status: string;
    gateway: string;
    externalPaymentId: string;
    paymentTime?: Date | string;
    subscriptionStartDate?: Date | string;
    subscriptionEndDate?: Date | string;
}

export interface IGetSubscriptionPaymentsInput {
    filter?: string;
    status?: string;
    sorting?: string;
    skipCount?: number;
    maxResultCount?: number;
}

export interface IPagedResultDtoOfSubscriptionPaymentDto {
    totalCount: number;
    items: ISubscriptionPaymentDto[];
}

export interface IPaymentGatewayDto {
    name: string;
    displayName: string;
    isConfigured: boolean;
    isDefault: boolean;
}

export interface IStripePaymentGatewaySettingsDto {
    secretKey?: string;
    publishableKey?: string;
    webhookSecret?: string;
}

export interface IPayPalPaymentGatewaySettingsDto {
    clientId?: string;
    clientSecret?: string;
    webhookId?: string;
}

export interface IMercadoPagoPaymentGatewaySettingsDto {
    accessToken?: string;
    publicKey?: string;
}

export interface IPagSeguroPaymentGatewaySettingsDto {
    token?: string;
    email?: string;
}

export interface IPaymentGatewaySettingsDto {
    defaultGateway?: string;
    stripe: IStripePaymentGatewaySettingsDto;
    payPal: IPayPalPaymentGatewaySettingsDto;
    mercadoPago: IMercadoPagoPaymentGatewaySettingsDto;
    pagSeguro: IPagSeguroPaymentGatewaySettingsDto;
}

@Injectable()
export class PaymentServiceProxy {
    private readonly http: HttpClient;
    private readonly baseUrl: string;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ?? '';
    }

    getAll(input: IGetSubscriptionPaymentsInput): Observable<IPagedResultDtoOfSubscriptionPaymentDto> {
        let url_ = this.baseUrl + '/api/services/app/Payment/GetAll?';
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

    createPayment(input: ICreateSubscriptionPaymentInput): Observable<IPaymentRequestDto> {
        const url_ = this.baseUrl + '/api/services/app/Payment/CreatePayment';
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processPaymentRequest(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    processPayment(paymentId: number, input: IProcessPaymentInput): Observable<ISubscriptionPaymentDto> {
        const url_ = this.baseUrl + '/api/services/app/Payment/ProcessPayment?paymentId=' + encodeURIComponent('' + paymentId);
        const options: unknown = { observe: 'response', responseType: 'json', body: input };
        return this.http.request('post', url_, options).pipe(_observableMergeMap((response: any) => this.processSubscriptionPayment(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    getGatewayList(): Observable<IPaymentGatewayDto[]> {
        const url_ = this.baseUrl + '/api/services/app/Payment/GetGatewayList';
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => {
            const status = response.status;
            const body = response.body ?? [];
            if (status === 200) {
                return _observableOf(body as IPaymentGatewayDto[]);
            }
            return _observableThrow(new Error('Unexpected response: ' + status));
        })).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    getGatewaySettings(): Observable<IPaymentGatewaySettingsDto> {
        const url_ = this.baseUrl + '/api/services/app/Payment/GetGatewaySettings';
        const options: unknown = { observe: 'response', responseType: 'json' };
        return this.http.request('get', url_, options).pipe(_observableMergeMap((response: any) => {
            const status = response.status;
            const body = response.body ?? {};
            if (status === 200) {
                return _observableOf(body as IPaymentGatewaySettingsDto);
            }
            return _observableThrow(new Error('Unexpected response: ' + status));
        })).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    updateGatewaySettings(input: IPaymentGatewaySettingsDto): Observable<void> {
        const url_ = this.baseUrl + '/api/services/app/Payment/UpdateGatewaySettings';
        const options: unknown = { body: input, headers: { 'Content-Type': 'application/json' }, observe: 'response', responseType: 'blob' };
        return this.http.request('put', url_, options).pipe(_observableMergeMap((response: any) => this.processAction(response))).pipe(_observableCatch((response: any) => {
            if (response instanceof Error) throw response;
            return _observableThrow(response);
        }));
    }

    private processPaged(response: HttpResponse<any>): Observable<IPagedResultDtoOfSubscriptionPaymentDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IPagedResultDtoOfSubscriptionPaymentDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    private processPaymentRequest(response: HttpResponse<any>): Observable<IPaymentRequestDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as IPaymentRequestDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    private processSubscriptionPayment(response: HttpResponse<any>): Observable<ISubscriptionPaymentDto> {
        const status = response.status;
        const responseBlob = response.body ?? new Blob();
        if (status === 200) {
            return _observableOf(responseBlob as ISubscriptionPaymentDto);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }

    private processAction(response: HttpResponse<any>): Observable<void> {
        const status = response.status;
        if (status === 200) {
            return _observableOf(undefined as any);
        }
        return _observableThrow(new Error('Unexpected response: ' + status));
    }
}
