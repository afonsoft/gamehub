import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { EditionServiceProxy, IUpdateEditionInput } from '@shared/service-proxies/edition.service-proxy';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
    standalone: false,
    selector: 'createOrEditEditionModal',
    templateUrl: './create-or-edit-edition-modal.component.html',
    animations: [appModuleAnimation()],
})
export class CreateOrEditEditionModalComponent extends AppComponentBase {
    @ViewChild('createOrEditModal', { static: true }) modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    edition: IUpdateEditionInput = {
        id: 0,
        displayName: '',
        isFree: false,
        monthlyPrice: undefined,
        annualPrice: undefined,
        quarterlyPrice: undefined,
        biannualPrice: undefined,
        permanentPrice: undefined,
        defaultPaymentPeriodType: 30,
        trialDayCount: undefined,
        waitingDayAfterExpire: undefined,
        expiringEditionId: undefined,
    };

    constructor(
        injector: Injector,
        private readonly _editionService: EditionServiceProxy,
    ) {
        super(injector);
    }

    show(editionId?: number): void {
        this.active = true;
        this.modal.show();

        if (editionId) {
            this._editionService.getEditionForEdit(editionId).subscribe(result => {
                this.edition = { ...result as IUpdateEditionInput };
            });
        } else {
            this.edition = {
                id: 0,
                displayName: '',
                isFree: false,
                monthlyPrice: undefined,
                annualPrice: undefined,
                quarterlyPrice: undefined,
                biannualPrice: undefined,
                permanentPrice: undefined,
                defaultPaymentPeriodType: 30,
                trialDayCount: undefined,
                waitingDayAfterExpire: undefined,
                expiringEditionId: undefined,
            };
        }
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    save(): void {
        if (!this.edition.displayName) {
            this.notify.warn(this.l('ThisFieldIsRequired'));
            return;
        }

        this.saving = true;
        const request = this.edition.id
            ? this._editionService.updateEdition(this.edition)
            : this._editionService.createEdition(this.edition);

        request.pipe(finalize(() => (this.saving = false))).subscribe(() => {
            this.notify.success(this.l('SavedSuccessfully'));
            this.close();
            this.modalSave.emit(null);
        });
    }
}
