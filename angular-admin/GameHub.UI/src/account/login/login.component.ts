import { Component, Injector, OnInit } from '@angular/core';
import { EafSessionService } from '@eaf/session/eaf-session.service';
import { accountModuleAnimation } from '@shared/animations/routerTransition';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';
import {
  AccountServiceProxy,
  SessionServiceProxy,
  TenantListDto,
  UpdateUserSignInTokenOutput,
} from '@shared/service-proxies/service-proxies';
import { ReCaptchaV3Service } from 'ngx-captcha';
import { UrlHelper } from 'shared/helpers/UrlHelper';

import { ExternalLoginProvider, LoginService } from './login.service';

@Component({
  standalone: false,
  templateUrl: './login.component.html',
  animations: [accountModuleAnimation()],
})
export class LoginComponent extends AppComponentBase implements OnInit {
  submitting = false;
  isMultiTenancyEnabled: boolean = this.multiTenancy.isEnabled;
  recaptchaSiteKey: string = AppConsts.recaptchaSiteKey;
  isSocialLogin = true;

  tenants: TenantListDto[] = [];
  selectedTenant: TenantListDto;

  get isTwoStepLogin(): boolean {
    return AppConsts.multiTenancy?.twoStepLogin ?? false;
  }

  constructor(
    injector: Injector,
    public loginService: LoginService,
    private readonly _sessionService: EafSessionService,
    private readonly _accountService: AccountServiceProxy,
    private readonly _sessionAppService: SessionServiceProxy,
    private readonly _reCaptchaV3Service: ReCaptchaV3Service,
  ) {
    super(injector);
  }

  ngOnInit(): void {
    this.dataTableHelper.showLoadingIndicator();
    this.submitting = true;
    this.clearSession();
    if (this.isMultiTenancyEnabled) {
      eaf.multiTenancy.setTenantIdCookie(null);

      this._accountService.getAllTenants().subscribe(result => {
        this.tenants = result;
        this.dataTableHelper.hideLoadingIndicator();
        eaf.ui.clearBusy();
      });
    } else {
      this.dataTableHelper.hideLoadingIndicator();
      eaf.ui.clearBusy();
    }

    if (this._sessionService.userId > 0 && UrlHelper.getReturnUrl() && UrlHelper.getSingleSignIn()) {
      this._sessionAppService.updateUserSignInToken().subscribe((result: UpdateUserSignInTokenOutput) => {
        const initialReturnUrl = UrlHelper.getReturnUrl();
        const returnUrl =
          initialReturnUrl +
          (initialReturnUrl.includes('?') ? '&' : '?') +
          'accessToken=' +
          result.signInToken +
          '&userId=' +
          result.encodedUserId +
          '&tenantId=' +
          result.encodedTenantId;

        location.href = returnUrl;
      });
    }
    this.handleExternalLoginCallbacks();
    this.loginService.init();
    this.submitting = false;
    this.dataTableHelper.hideLoadingIndicator();
  }
  clearSession() {
    eaf.utils.deleteCookie(AppConsts.authorization.encrptedAuthTokenName, eaf.appPath);
    eaf.utils.deleteCookie(eaf.auth.tokenCookieName, eaf.appPath);
    eaf.utils.deleteCookie(eaf.multiTenancy.tenantIdCookieName, eaf.appPath);
    eaf.auth.clearToken();
  }

  ChangeLogin(): void {
    this.isSocialLogin = !this.isSocialLogin;
  }

  handleExternalLoginCallbacks(): void {
    const state = UrlHelper.getQueryParametersUsingHash().state;
    const parameters = UrlHelper.getQueryParameters();
    this.submitting = true;
    if (state?.includes('openIdConnect') || parameters['openIdConnect'] !== undefined) {
      this.loginService.openIdConnectLoginCallback();
    }
  }

  login(): void {
    if (this.isTwoStepLogin) {
      this.twoStepLogin();
      return;
    }

    this.normalLogin();
  }

  private normalLogin(): void {
    const recaptchaCallback = (token: string) => {
      this.submitting = true;
      this.loginService.authenticate(
        () => {
          this.submitting = false;
          this.dataTableHelper.hideLoadingIndicator();
        },
        null,
        token,
      );
    };

    if (this.useCaptcha) {
      this._reCaptchaV3Service.execute(this.recaptchaSiteKey, 'login', token => {
        recaptchaCallback(token);
      });
    } else {
      recaptchaCallback(null);
    }
  }

  private twoStepLogin(): void {
    const recaptchaCallback = (token: string) => {
      this.submitting = true;
      this.dataTableHelper.showLoadingIndicator();

      const model = {
        userNameOrEmailAddress: this.loginService.authenticateModel.userNameOrEmailAddress,
        password: this.loginService.authenticateModel.password,
      };

      this.loginService
        .availableTenants(model)
        .subscribe({
          next: tenants => {
            this.loginService.availableTenantsResult = tenants;

            if (tenants.length === 0) {
              this.loginService.authenticate(
                () => {
                  this.submitting = false;
                  this.dataTableHelper.hideLoadingIndicator();
                },
                undefined,
                token,
              );
            } else if (tenants.length === 1 && AppConsts.autoSelectSingleTenant) {
              this.loginService
                .selectTenant({
                  ...model,
                  tenantId: tenants[0].tenantId,
                })
                .subscribe({
                  next: result => this.loginService.loginTenant(result, tenants[0].tenantId),
                  error: () => {
                    this.submitting = false;
                    this.dataTableHelper.hideLoadingIndicator();
                  },
                });
            } else {
              this.submitting = false;
              this.dataTableHelper.hideLoadingIndicator();
              this.loginService.navigateToSelectTenant();
            }
          },
          error: () => {
            this.submitting = false;
            this.dataTableHelper.hideLoadingIndicator();
          },
        });
    };

    if (this.useCaptcha) {
      this._reCaptchaV3Service.execute(this.recaptchaSiteKey, 'login', token => {
        recaptchaCallback(token);
      });
    } else {
      recaptchaCallback(null);
    }
  }

  selectTenant(tenant: TenantListDto): void {
    this.selectedTenant = tenant;
    eaf.multiTenancy.setTenantIdCookie(this.selectedTenant.id);
  }

  selectHost(): void {
    this.selectedTenant = undefined;
    eaf.multiTenancy.setTenantIdCookie(null);
  }

  truncateStringWithPostfix(name: string): string {
    return eaf.utils.truncateStringWithPostfix(name, 30);
  }

  get useCaptcha(): boolean {
    return this.setting.getBoolean('App.UserManagement.UseCaptchaOnLogin');
  }

  externalLogin(provider: ExternalLoginProvider) {
    this.dataTableHelper.showLoadingIndicator();
    this.submitting = true;
    this.loginService.externalAuthenticate(provider);
  }
}
