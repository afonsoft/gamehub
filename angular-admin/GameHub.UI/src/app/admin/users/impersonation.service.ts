import { Injectable } from '@angular/core';
import { AppAuthService } from '@app/shared/common/auth/app-auth.service';
import { AppUrlService } from '@shared/common/nav/app-url.service';
import { AccountServiceProxy, ImpersonateInput, ImpersonateOutput } from '@shared/service-proxies/service-proxies';

@Injectable()
export class ImpersonationService {
  constructor(
    private readonly _accountService: AccountServiceProxy,
    private readonly _appUrlService: AppUrlService,
    private readonly _authService: AppAuthService,
  ) {}

  impersonate(userId: number, tenantId?: number): void {
    const input = new ImpersonateInput();
    input.userId = userId;
    input.tenantId = tenantId;

    this._accountService.impersonate(input).subscribe((result: ImpersonateOutput) => {
      this._authService.logout(false);

      let targetUrl = this._appUrlService.getAppRootUrlOfTenant(result.tenancyName) + '?impersonationToken=' + result.impersonationToken;
      if (input.tenantId) {
        targetUrl = targetUrl + '&tenantId=' + input.tenantId;
      }

      location.href = targetUrl;
    });
  }

  backToImpersonator(): void {
    this._accountService.backToImpersonator().subscribe((result: ImpersonateOutput) => {
      this._authService.logout(false);

      let targetUrl = this._appUrlService.getAppRootUrlOfTenant(result.tenancyName) + '?impersonationToken=' + result.impersonationToken;
      if (eaf.session.impersonatorTenantId) {
        targetUrl = targetUrl + '&tenantId=' + eaf.session.impersonatorTenantId;
      }

      location.href = targetUrl;
    });
  }
}
