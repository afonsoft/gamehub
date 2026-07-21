import { Component, EventEmitter, Injector, Output, ViewChild } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
  CreateTenantInput,
  PasswordComplexitySetting,
  ProfileServiceProxy,
  TenantServiceProxy,
} from '@shared/service-proxies/service-proxies';

import { ModalDirective } from 'ngx-bootstrap/modal';
import { finalize } from 'rxjs/operators';

@Component({
  standalone: false,
  selector: 'createTenantModal',
  templateUrl: './create-tenant-modal.component.html',
})
export class CreateTenantModalComponent extends AppComponentBase {
  @ViewChild('createModal', { static: true }) modal: ModalDirective;

  @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

  active = false;
  saving = false;
  setRandomPassword = true;

  tenant: CreateTenantInput;
  passwordComplexitySetting: PasswordComplexitySetting = new PasswordComplexitySetting();

  constructor(
    injector: Injector,
    private readonly _tenantService: TenantServiceProxy,
    private readonly _profileService: ProfileServiceProxy,
  ) {
    super(injector);
  }

  show() {
    this.active = true;
    this.init();

    this._profileService.getPasswordComplexitySetting().subscribe(result => {
      this.passwordComplexitySetting = result.setting;
      this.modal.show();
    });
  }

  onShown(): void {
    document.getElementById('TenancyName').focus();
  }

  init(): void {
    this.tenant = new CreateTenantInput();
    this.tenant.isActive = true;
    this.tenant.shouldChangePasswordOnNextLogin = true;
    this.setRandomPassword = true;
    this.passwordComplexitySetting = new PasswordComplexitySetting();
    this.tenant.sendActivationEmail = false;
  }

  save(): void {
    this.saving = true;

    if (this.setRandomPassword) {
      this.tenant.adminPassword = null;
    }

    this._tenantService
      .createTenant(this.tenant)
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
