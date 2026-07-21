import { Component, ElementRef, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TenantEditDto, TenantServiceProxy } from '@shared/service-proxies/service-proxies';


import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'editTenantModal',
  templateUrl: './edit-tenant-modal.component.html',
})
export class EditTenantModalComponent extends AppComponentBase {
  @ViewChild('nameInput', { static: true }) nameInput: ElementRef;
  @ViewChild('editModal', { static: true }) modal: ModalDirective;
  @ViewChild('SubscriptionEndDateUtc', { static: true }) subscriptionEndDateUtc: ElementRef;

  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  active = false;
  saving = false;

  tenant: TenantEditDto = undefined;

  constructor(
    injector: Injector,
    private readonly _tenantService: TenantServiceProxy,
  ) {
    super(injector);
  }

  show(tenantId: number): void {
    this.active = true;

    this._tenantService.getTenantForEdit(tenantId).subscribe(tenantResult => {
      this.tenant = tenantResult;
      this.modal.show();
    });
  }

  onShown(): void {
    document.getElementById('Name').focus();
  }

  save(): void {
    this.saving = true;

    this._tenantService
      .updateTenant(this.tenant)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.notify.success(this.l('SavedSuccessfully'));
        this.close();
        this.modalSave.emit(null);
      });
  }

  close(): void {
    this.active = false;
    this.modal.hide();
  }
}
