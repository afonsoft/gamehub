import { Injectable, Injector } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';

export interface ISubscriptionPaymentProductInput {
    description?: string;
    count?: number;
    amount?: number;
}

export interface ICreatePaymentRequest {
    editionId: number;
    editionPaymentType?: number;
    paymentPeriodType?: number;
    gateway?: string;
    isRecurring?: boolean;
    successUrl?: string;
    errorUrl?: string;
    description?: string;
    products?: ISubscriptionPaymentProductInput[];
}

export interface IPaymentRequestResult {
    subscriptionPaymentId?: number;
    paymentId?: string;
    gatewayPaymentId?: string;
    gateway?: string;
    checkoutUrl?: string;
    isSuccess?: boolean;
    errorMessage?: string;
}

export interface ISubscriptionPaymentProductDto {
    description?: string;
    count?: number;
    amount?: number;
}

export interface ISubscriptionPaymentDto {
    id?: number;
    tenantId?: number;
    editionId?: number;
    editionPaymentType?: number;
    paymentPeriodType?: number;
    amount?: number;
    status?: string;
    isRecurring?: boolean;
    isProrationPayment?: boolean;
    gateway?: string;
    externalPaymentId?: string;
    gatewaySubscriptionId?: string;
    invoiceNo?: string;
    description?: string;
    successUrl?: string;
    errorUrl?: string;
    paymentTime?: string;
    subscriptionStartDate?: string;
    subscriptionEndDate?: string;
    products?: ISubscriptionPaymentProductDto[];
}

export interface IGetSubscriptionPaymentsInput {
    filter?: string;
    sorting?: string;
    skipCount?: number;
    maxResultCount?: number;
}

export interface IPagedResultDto<T> {
    totalCount?: number;
    items?: T[];
}

export interface IUpgradeSubscriptionInput {
    tenantId?: number;
    newEditionId: number;
    paymentPeriodType?: number;
    gateway?: string;
}

@Injectable()
export class PaymentExtendedService extends AppComponentBase {
    private readonly _paymentUrl = `${AppConsts.remoteServiceBaseUrl || ''}/api/services/app/Payment`;

    constructor(
        injector: Injector,
        private readonly _httpClient: HttpClient,
    ) {
        super(injector);
    }

    getAll(input: IGetSubscriptionPaymentsInput): Observable<IPagedResultDto<ISubscriptionPaymentDto>> {
        let params = new HttpParams();
        if (input.filter !== undefined && input.filter !== null) {
            params = params.set('Filter', input.filter);
        }
        if (input.sorting !== undefined && input.sorting !== null) {
            params = params.set('Sorting', input.sorting);
        }
        if (input.skipCount !== undefined && input.skipCount !== null) {
            params = params.set('SkipCount', input.skipCount.toString());
        }
        if (input.maxResultCount !== undefined && input.maxResultCount !== null) {
            params = params.set('MaxResultCount', input.maxResultCount.toString());
        }

        return this._httpClient.get<IPagedResultDto<ISubscriptionPaymentDto>>(`${this._paymentUrl}/GetAll`, { params });
    }

    getPayment(id: number): Observable<ISubscriptionPaymentDto> {
        const params = new HttpParams().set('id', id.toString());
        return this._httpClient.get<ISubscriptionPaymentDto>(`${this._paymentUrl}/GetPayment`, { params });
    }

    createPayment(input: ICreatePaymentRequest): Observable<IPaymentRequestResult> {
        return this._httpClient.post<IPaymentRequestResult>(`${this._paymentUrl}/CreatePayment`, input);
    }

    upgradeSubscription(input: IUpgradeSubscriptionInput): Observable<IPaymentRequestResult> {
        return this._httpClient.post<IPaymentRequestResult>(`${this._paymentUrl}/UpgradeSubscription`, input);
    }

    cancelRecurring(paymentId: number): Observable<ISubscriptionPaymentDto> {
        const params = new HttpParams().set('paymentId', paymentId.toString());
        return this._httpClient.post<ISubscriptionPaymentDto>(`${this._paymentUrl}/CancelRecurring`, null, { params });
    }
}
