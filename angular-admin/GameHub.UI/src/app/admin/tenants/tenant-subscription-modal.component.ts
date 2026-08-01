import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EditionServiceProxy, IEditionDto } from '@shared/service-proxies/edition.service-proxy';
import { TenantSubscriptionServiceProxy, ITenantSubscriptionDto, IAssignEditionToTenantInput, IExtendTenantSubscriptionInput } from '@shared/service-proxies/tenant-subscription.service-proxy';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
    standalone: false,
    selector: 'tenantSubscriptionModal',
    templateUrl: './tenant-subscription-modal.component.html',
    animations: [appModuleAnimation()],
})
export class TenantSubscriptionModalComponent extends AppComponentBase {
    @ViewChild('tenantSubscriptionModal', { static: true }) modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    tenantId: number;
    tenantName: string;
    subscription: ITenantSubscriptionDto;
    editions: IEditionDto[] = [];

    assignInput: IAssignEditionToTenantInput = { tenantId: 0, editionId: 0, paymentPeriodType: 30 };
    extendInput: IExtendTenantSubscriptionInput = { tenantId: 0, paymentPeriodType: 30 };

    constructor(
        injector: Injector,
        private readonly _tenantSubscriptionService: TenantSubscriptionServiceProxy,
        private readonly _editionService: EditionServiceProxy,
    ) {
        super(injector);
    }

    show(tenantId: number, tenantName: string): void {
        this.active = true;
        this.tenantId = tenantId;
        this.tenantName = tenantName;
        this.assignInput = { tenantId, editionId: 0, paymentPeriodType: 30 };
        this.extendInput = { tenantId, paymentPeriodType: 30 };
        this.modal.show();
        this.loadEditions();
        this.loadSubscription();
    }

    loadEditions(): void {
        this._editionService.getEditions('', 'displayName', 1000, 0).subscribe(result => {
            this.editions = result.items ?? [];
        });
    }

    loadSubscription(): void {
        this._tenantSubscriptionService.getTenantSubscription(this.tenantId).subscribe(result => {
            this.subscription = result;
            if (result.editionId && this.editions.length > 0) {
                this.assignInput.editionId = result.editionId;
            }
        });
    }

    assignEdition(): void {
        if (this.assignInput.editionId === 0) {
            this.notify.warn(this.l('ThisFieldIsRequired'));
            return;
        }

        this.saving = true;
        this._tenantSubscriptionService
            .assignEditionToTenant(this.assignInput)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.loadSubscription();
                this.modalSave.emit(null);
            });
    }

    extendSubscription(): void {
        this.saving = true;
        this._tenantSubscriptionService
            .extendTenantSubscription(this.extendInput)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.loadSubscription();
                this.modalSave.emit(null);
            });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    getEditionName(editionId?: number): string {
        return this.editions.find(e => e.id === editionId)?.displayName ?? String(editionId ?? '');
    }
}
