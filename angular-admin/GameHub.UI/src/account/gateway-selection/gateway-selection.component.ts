import { Component, Injector, OnInit } from '@angular/core';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import {
    EditionServiceProxy,
    IEditionDto,
    IPaymentGatewayDto,
    PaymentServiceProxy,
} from '@shared/service-proxies/service-proxies';
import {
    ICreatePaymentRequest,
    IPaymentRequestResult,
    ISubscriptionPaymentDto,
    PaymentExtendedService,
} from '@shared/service-proxies/payment-extended.service';

@Component({
    standalone: false,
    selector: 'app-gateway-selection',
    templateUrl: './gateway-selection.component.html',
    animations: [accountModuleAnimation()],
})
export class GatewaySelectionComponent extends AppComponentBase implements OnInit {
    model: ICreatePaymentRequest = {
        editionId: 0,
        editionPaymentType: 1,
        paymentPeriodType: 30,
        gateway: '',
        isRecurring: false,
        description: '',
    };
    editions: IEditionDto[] = [];
    gateways: IPaymentGatewayDto[] = [];
    saving = false;
    resultType: 'success' | 'error' | '' = '';
    resultMessage = '';
    payment: ISubscriptionPaymentDto | null = null;

    constructor(
        injector: Injector,
        private readonly _editionService: EditionServiceProxy,
        private readonly _paymentService: PaymentServiceProxy,
        private readonly _paymentExtendedService: PaymentExtendedService,
        private readonly _activatedRoute: ActivatedRoute,
        private readonly _router: Router,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.loadEditions();
        this.loadGateways();
        this._activatedRoute.queryParams.subscribe(params => this.handleQueryParams(params));
    }

    private loadEditions(): void {
        this._editionService.getEditions('', 'displayName', 0, 1000).subscribe(result => {
            this.editions = result.items ?? [];
            if (this.editions.length > 0 && this.model.editionId === 0 && !this.payment) {
                this.model.editionId = this.editions[0].id;
            }
        });
    }

    private loadGateways(): void {
        this._paymentService.getGatewayList().subscribe(result => {
            this.gateways = result ?? [];
            if (this.gateways.length > 0 && !this.model.gateway) {
                const defaultGateway = this.gateways.find(g => g.isDefault);
                this.model.gateway = defaultGateway?.name ?? this.gateways[0].name ?? '';
            }
        });
    }

    private handleQueryParams(params: { [key: string]: string }): void {
        if (params.status) {
            this.resultType = params.status === 'success' ? 'success' : 'error';
            this.resultMessage = this.resultType === 'success'
                ? this.l('PaymentSuccessful')
                : this.l('PaymentFailed');
        }

        if (params.paymentId) {
            this.loadPayment(+params.paymentId);
        } else if (params.session_id) {
            // Stripe redirect without paymentId should not happen because
            // successUrl contains the paymentId placeholder.
            this.resultType = 'error';
            this.resultMessage = this.l('PaymentReturnMissingPaymentId');
        }
    }

    private loadPayment(id: number): void {
        this.saving = true;
        this._paymentExtendedService.getPayment(id)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(payment => {
                this.payment = payment;
                this.model.editionId = payment.editionId ?? 0;
                this.model.paymentPeriodType = payment.paymentPeriodType ?? 30;
                this.model.gateway = payment.gateway ?? '';
                this.model.isRecurring = payment.isRecurring ?? false;
                this.model.description = payment.description ?? '';
            });
    }

    getSelectedEdition(): IEditionDto | undefined {
        return this.editions.find(e => e.id === this.model.editionId);
    }

    getAmountPreview(): number | undefined {
        const edition = this.getSelectedEdition();
        if (!edition || edition.isFree) {
            return undefined;
        }

        switch (this.model.paymentPeriodType) {
            case 1:
            case 7:
                return undefined;
            case 30:
                return edition.monthlyPrice;
            case 90:
                return edition.quarterlyPrice;
            case 180:
                return edition.biannualPrice;
            case 365:
                return edition.annualPrice;
            case 99999:
                return edition.permanentPrice;
            default:
                return undefined;
        }
    }

    isRecurringSupported(): boolean {
        return this.model.gateway === 'Stripe';
    }

    createPayment(): void {
        if (this.model.editionId === 0) {
            this.notify.warn(this.l('EditionIsRequired'));
            return;
        }

        if (!this.model.gateway) {
            this.notify.warn(this.l('GatewayIsRequired'));
            return;
        }

        if (this.model.isRecurring && !this.isRecurringSupported()) {
            this.notify.warn(this.l('GatewayDoesNotSupportRecurring'));
            return;
        }

        const base = window.location.origin;
        const input: ICreatePaymentRequest = {
            ...this.model,
            successUrl: `${base}/account/gateway-selection?status=success&paymentId={paymentId}&session_id={CHECKOUT_SESSION_ID}`,
            errorUrl: `${base}/account/gateway-selection?status=error&paymentId={paymentId}`,
        };

        this.saving = true;
        this._paymentExtendedService.createPayment(input)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe((result: IPaymentRequestResult) => {
                if (result.isSuccess && result.subscriptionPaymentId) {
                    if (result.checkoutUrl) {
                        window.location.href = result.checkoutUrl;
                        return;
                    }

                    this._router.navigate(['/account/gateway-selection'], {
                        queryParams: { paymentId: result.subscriptionPaymentId, status: 'created' },
                    });
                    return;
                }

                this.notify.error(result.errorMessage ?? this.l('PaymentCreationFailed'));
            });
    }
}
