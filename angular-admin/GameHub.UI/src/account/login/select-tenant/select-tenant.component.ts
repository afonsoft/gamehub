import { Component, Injector } from '@angular/core';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { LoginService, AvailableTenantResult } from '../login.service';

@Component({
  standalone: false,
  templateUrl: './select-tenant.component.html',
  animations: [accountModuleAnimation()],
})
export class SelectTenantComponent extends AppComponentBase {
  tenants: AvailableTenantResult[] = [];
  submitting = false;

  constructor(
    injector: Injector,
    public loginService: LoginService,
  ) {
    super(injector);
    this.tenants = this.loginService.availableTenantsResult || [];
  }

  select(tenant: AvailableTenantResult): void {
    if (this.submitting) {
      return;
    }

    this.submitting = true;
    this.dataTableHelper.showLoadingIndicator();

    const model = {
      userNameOrEmailAddress: this.loginService.authenticateModel.userNameOrEmailAddress,
      password: this.loginService.authenticateModel.password,
      tenantId: tenant.tenantId,
    };

    this.loginService.selectTenant(model).subscribe(
      result => {
        this.loginService.loginTenant(result, tenant.tenantId);
      },
      () => {
        this.submitting = false;
        this.dataTableHelper.hideLoadingIndicator();
      },
    );
  }

  loginAsHost(): void {
    if (this.submitting) {
      return;
    }

    this.submitting = true;
    this.dataTableHelper.showLoadingIndicator();

    eaf.multiTenancy.setTenantIdCookie(null);
    this.loginService.authenticate(() => {
      this.submitting = false;
      this.dataTableHelper.hideLoadingIndicator();
    });
  }
}
