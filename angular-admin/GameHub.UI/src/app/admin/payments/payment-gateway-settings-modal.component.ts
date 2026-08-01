import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { IPaymentGatewaySettingsDto, PaymentGatewaySettingsDto, PaymentServiceProxy } from '@shared/service-proxies/service-proxies';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
    standalone: false,
    selector: 'paymentGatewaySettingsModal',
    templateUrl: './payment-gateway-settings-modal.component.html',
    animations: [appModuleAnimation()],
})
export class PaymentGatewaySettingsModalComponent extends AppComponentBase {
    @ViewChild('paymentGatewaySettingsModal', { static: true }) modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    settings: IPaymentGatewaySettingsDto;
    gateways: string[] = ['Stripe', 'PayPal', 'MercadoPago', 'PagSeguro'];

    constructor(
        injector: Injector,
        private readonly _paymentService: PaymentServiceProxy,
    ) {
        super(injector);
    }

    show(): void {
        this.active = true;
        this.modal.show();
        this._paymentService.getGatewaySettings().subscribe(result => {
            this.settings = result;
        });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    save(): void {
        this.saving = true;
        this._paymentService
            .updateGatewaySettings(new PaymentGatewaySettingsDto(this.settings as any))
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }
}
