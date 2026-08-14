import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
    PaymentServiceProxy,
    PaymentGatewaySettingsDto,
    StripePaymentGatewaySettingsDto,
    PayPalPaymentGatewaySettingsDto,
    MercadoPagoPaymentGatewaySettingsDto,
    PagSeguroPaymentGatewaySettingsDto,
} from '@shared/service-proxies/service-proxies';
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
    settings: PaymentGatewaySettingsDto;
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
            this.ensureGatewaySettings();
        });
    }

    private ensureGatewaySettings(): void {
        if (!this.settings) {
            return;
        }

        if (!this.settings.stripe) {
            this.settings.stripe = new StripePaymentGatewaySettingsDto();
        }
        if (!this.settings.payPal) {
            this.settings.payPal = new PayPalPaymentGatewaySettingsDto();
        }
        if (!this.settings.mercadoPago) {
            this.settings.mercadoPago = new MercadoPagoPaymentGatewaySettingsDto();
        }
        if (!this.settings.pagSeguro) {
            this.settings.pagSeguro = new PagSeguroPaymentGatewaySettingsDto();
        }
    }

    isGatewayConfigured(name: string): boolean {
        if (!this.settings) {
            return false;
        }

        switch (name) {
            case 'Stripe':
                return !!this.settings.stripe?.secretKey;
            case 'PayPal':
                return !!this.settings.payPal?.clientId;
            case 'MercadoPago':
                return !!this.settings.mercadoPago?.accessToken;
            case 'PagSeguro':
                return !!this.settings.pagSeguro?.token && !!this.settings.pagSeguro?.email;
            default:
                return false;
        }
    }

    isSettingsValid(): boolean {
        if (!this.settings) {
            return false;
        }

        if (!this.settings.defaultGateway) {
            return true;
        }

        return this.isGatewayConfigured(this.settings.defaultGateway);
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    save(): void {
        if (!this.isSettingsValid()) {
            this.notify.warn(this.l('DefaultGatewayMustBeConfigured'));
            return;
        }

        this.saving = true;
        this._paymentService
            .updateGatewaySettings(this.settings)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe(() => {
                this.notify.success(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }
}
