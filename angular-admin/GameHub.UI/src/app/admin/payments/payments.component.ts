import { Component, Injector, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
  CreateSubscriptionPaymentInput,
  EditionServiceProxy,
  ICreateSubscriptionPaymentInput,
  IEditionDto,
  IPaymentGatewayDto,
  IProcessPaymentInput,
  ISubscriptionPaymentDto,
  PaymentServiceProxy,
  ProcessPaymentInput,
} from '@shared/service-proxies/service-proxies';
import { PaymentGatewaySettingsModalComponent } from './payment-gateway-settings-modal.component';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { LazyLoadEvent } from 'primeng/api';
import { Paginator } from 'primeng/paginator';
import { Table } from 'primeng/table';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  templateUrl: './payments.component.html',
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()],
})
export class PaymentsComponent extends AppComponentBase implements OnInit {
  @ViewChild('dataTable', { static: true }) dataTable: Table;
  @ViewChild('paginator', { static: true }) paginator: Paginator;
  @ViewChild('createModal', { static: true }) createModal: ModalDirective;
  @ViewChild('processModal', { static: true }) processModal: ModalDirective;
  @ViewChild('paymentGatewaySettingsModal', { static: true }) paymentGatewaySettingsModal: PaymentGatewaySettingsModalComponent;

  filters: { filterText: string; status: string } = { filterText: '', status: '' };
  payments: ISubscriptionPaymentDto[] = [];
  editions: IEditionDto[] = [];
  gateways: IPaymentGatewayDto[] = [];
  saving = false;

  newPayment: ICreateSubscriptionPaymentInput & { isRecurring: boolean } = {
    editionId: 0,
    editionPaymentType: 1,
    paymentPeriodType: 30,
    gateway: '',
    description: undefined,
    isRecurring: false,
  };

  processInput: IProcessPaymentInput & { paymentId: number } = {
    paymentId: 0,
    externalPaymentId: '',
    gateway: '',
    gatewayResponse: '',
    isSuccess: true,
  };

  constructor(
    injector: Injector,
    private readonly _paymentService: PaymentServiceProxy,
    private readonly _editionService: EditionServiceProxy,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.resetFilters();
    this.loadEditions();
    this.loadGateways();
  }

  resetFilters(): void {
    this.filters = { filterText: '', status: '' };
  }

  loadEditions(): void {
    this._editionService.getEditions('', 'displayName', 0, 1000).subscribe(result => {
      this.editions = result.items ?? [];
      if (this.editions.length > 0 && this.newPayment.editionId === 0) {
        this.newPayment.editionId = this.editions[0].id;
      }
    });
  }

  loadGateways(): void {
    this._paymentService.getGatewayList().subscribe(result => {
      this.gateways = result ?? [];
      const defaultGateway = this.gateways.find(g => g.isDefault);
      if (this.newPayment.gateway === '') {
        this.newPayment.gateway = defaultGateway?.name ?? (this.gateways.length > 0 ? this.gateways[0].name : '');
      }
    });
  }

  getPayments(event?: LazyLoadEvent): void {
    if (this.dataTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.dataTableHelper.showLoadingIndicator();
    this._paymentService
      .getAll(
        this.filters.filterText,
        this.dataTableHelper.getSorting(this.dataTable),
        this.dataTableHelper.getSkipCount(this.paginator, event),
        this.dataTableHelper.getMaxResultCount(this.paginator, event),
      )
      .pipe(finalize(() => this.dataTableHelper.hideLoadingIndicator()))
      .subscribe(result => {
        this.dataTableHelper.totalRecordsCount = result.totalCount;
        this.dataTableHelper.records = result.items;
      });
  }

  showGatewaySettings(): void {
    this.paymentGatewaySettingsModal.show();
  }

  showCreateModal(): void {
    this.newPayment = {
      editionId: this.editions.length > 0 ? this.editions[0].id : 0,
      editionPaymentType: 1,
      paymentPeriodType: 30,
      gateway: this.gateways.find(g => g.isDefault)?.name ?? (this.gateways.length > 0 ? this.gateways[0].name : ''),
      description: undefined,
      isRecurring: false,
    };
    this.createModal.show();
  }

  getSelectedEdition(): IEditionDto | undefined {
    return this.editions.find(e => e.id === this.newPayment.editionId);
  }

  getAmountPreview(): number | undefined {
    const edition = this.getSelectedEdition();
    if (!edition) {
      return undefined;
    }

    switch (this.newPayment.paymentPeriodType) {
      case 1:
        return undefined;
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

  isRecurringSupported(gatewayName: string): boolean {
    return gatewayName === 'Stripe';
  }

  closeCreateModal(): void {
    this.createModal.hide();
  }

  savePayment(): void {
    if (this.newPayment.editionId === 0) {
      this.notify.warn(this.l('ThisFieldIsRequired'));
      return;
    }

    if (!this.newPayment.gateway) {
      this.notify.warn(this.l('GatewayIsRequired'));
      return;
    }

    if (this.newPayment.isRecurring && !this.isRecurringSupported(this.newPayment.gateway)) {
      this.notify.warn(this.l('GatewayDoesNotSupportRecurring'));
      return;
    }

    this.saving = true;
    const input = { ...this.newPayment };
    this._paymentService
      .createPayment(input as any)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.notify.success(this.l('SavedSuccessfully'));
        this.closeCreateModal();
        this.getPayments();
      });
  }

  getEditionDisplayName(editionId: number): string {
    return this.editions.find(e => e.id === editionId)?.displayName ?? String(editionId);
  }

  getStatusClass(status: string): string {
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

  getStatusLabel(status: string): string {
    return this.l(status) ?? status;
  }

  showProcessModal(payment: ISubscriptionPaymentDto): void {
    this.processInput = {
      paymentId: payment.id,
      externalPaymentId: payment.externalPaymentId ?? '',
      gateway: payment.gateway ?? '',
      gatewayResponse: '',
      isSuccess: true,
    };
    this.processModal.show();
  }

  closeProcessModal(): void {
    this.processModal.hide();
  }

  processPayment(): void {
    this.saving = true;
    const { paymentId, ...processInput } = this.processInput;
    this._paymentService
      .processPayment(paymentId, new ProcessPaymentInput(processInput as any))
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.notify.success(this.l('SavedSuccessfully'));
        this.closeProcessModal();
        this.getPayments();
      });
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }
}
