import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
    EditionServiceProxy,
    IEditionDto,
    IPaymentGatewayDto,
    PaymentServiceProxy,
} from '@shared/service-proxies/service-proxies';
import {
    IPagedResultDto,
    ISubscriptionPaymentDto,
    IUpgradeSubscriptionInput,
    PaymentExtendedService,
} from '@shared/service-proxies/payment-extended.service';
import { LazyLoadEvent } from 'primeng/api';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';

@Component({
    standalone: false,
    templateUrl: './subscriptions.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()],
})
export class SubscriptionsComponent extends AppComponentBase implements OnInit {
    @ViewChild('dataTable', { static: true }) dataTable: Table;
    @ViewChild('paginator', { static: true }) paginator: Paginator;
    @ViewChild('upgradeModal', { static: true }) upgradeModal: ModalDirective;

    filters: { filterText: string; status: string } = { filterText: '', status: '' };
    payments: ISubscriptionPaymentDto[] = [];
    editions: IEditionDto[] = [];
    gateways: IPaymentGatewayDto[] = [];
    saving = false;

    upgradePayment: ISubscriptionPaymentDto | null = null;
    upgradeEditionId = 0;
    upgradePeriod = 30;
    upgradeGateway = '';

    constructor(
        injector: Injector,
        private readonly _paymentExtendedService: PaymentExtendedService,
        private readonly _editionService: EditionServiceProxy,
        private readonly _paymentService: PaymentServiceProxy,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        this.loadEditions();
        this.loadGateways();
        this.resetFilters();
    }

    resetFilters(): void {
        this.filters = { filterText: '', status: '' };
    }

    loadEditions(): void {
        this._editionService.getEditions('', 'displayName', 0, 1000).subscribe(result => {
            this.editions = result.items ?? [];
        });
    }

    loadGateways(): void {
        this._paymentService.getGatewayList().subscribe(result => {
            this.gateways = result ?? [];
        });
    }

    getPayments(event?: LazyLoadEvent): void {
        if (this.dataTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.dataTableHelper.showLoadingIndicator();
        this._paymentExtendedService
            .getAll({
                filter: this.filters.filterText,
                sorting: this.dataTableHelper.getSorting(this.dataTable),
                skipCount: this.dataTableHelper.getSkipCount(this.paginator, event),
                maxResultCount: this.dataTableHelper.getMaxResultCount(this.paginator, event),
            })
            .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
            .subscribe((result: IPagedResultDto<ISubscriptionPaymentDto>) => {
                this.dataTableHelper.totalRecordsCount = result.totalCount ?? 0;
                this.payments = result.items ?? [];
                this.dataTableHelper.records = this.payments;
            });
    }

    getEditionDisplayName(editionId?: number): string {
        return this.editions.find(e => e.id === editionId)?.displayName ?? String(editionId ?? '');
    }

    getStatusClass(status?: string): string {
        switch (status) {
            case 'Pending':
                return 'badge badge-warning';
            case 'Processing':
                return 'badge badge-info';
            case 'Completed':
                return 'badge badge-success';
            case 'Canceled':
                return 'badge badge-secondary';
            case 'Failed':
                return 'badge badge-danger';
            default:
                return 'badge badge-light';
        }
    }

    getStatusLabel(status?: string): string {
        return this.l(status ?? '') ?? status;
    }

    showUpgradeModal(payment: ISubscriptionPaymentDto): void {
        this.upgradePayment = payment;
        this.upgradeEditionId = payment.editionId ?? 0;
        this.upgradePeriod = payment.paymentPeriodType ?? 30;
        this.upgradeGateway = payment.gateway ?? '';
        this.upgradeModal.show();
    }

    closeUpgradeModal(): void {
        this.upgradeModal.hide();
        this.upgradePayment = null;
    }

    upgradeSubscription(): void {
        if (!this.upgradePayment || this.upgradeEditionId === 0) {
            this.notify.warn(this.l('EditionIsRequired'));
            return;
        }

        if (!this.upgradeGateway) {
            this.notify.warn(this.l('GatewayIsRequired'));
            return;
        }

        const input: IUpgradeSubscriptionInput = {
            tenantId: this.upgradePayment.tenantId,
            newEditionId: this.upgradeEditionId,
            paymentPeriodType: this.upgradePeriod,
            gateway: this.upgradeGateway,
        };

        this.saving = true;
        this._paymentExtendedService
            .upgradeSubscription(input)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.closeUpgradeModal();
                this.getPayments();
            });
    }

    cancelRecurring(payment: ISubscriptionPaymentDto): void {
        if (!payment.id) {
            return;
        }

        this.message.confirm(
            this.l('CancelRecurringWarning', payment.externalPaymentId ?? payment.id.toString()),
            this.l('AreYouSure'),
            isConfirmed => {
                if (!isConfirmed) {
                    return;
                }

                this.saving = true;
                this._paymentExtendedService
                    .cancelRecurring(payment.id!)
                    .pipe(finalize(() => (this.saving = false)))
                    .subscribe(() => {
                        this.notify.success(this.l('SavedSuccessfully'));
                        this.getPayments();
                    });
            },
        );
    }

    canCancelRecurring(payment: ISubscriptionPaymentDto): boolean {
        return payment.isRecurring === true && (payment.status === 'Completed' || payment.status === 'Processing');
    }

    canUpgrade(payment: ISubscriptionPaymentDto): boolean {
        return payment.status === 'Completed' || payment.status === 'Processing';
    }
}
