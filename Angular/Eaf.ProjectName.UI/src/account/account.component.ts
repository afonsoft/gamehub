import { Component, Injector, OnInit, ViewContainerRef, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { StorageService } from '@eaf/utils/storage.service';
import { AppConsts } from '@shared/AppConsts';
import { AppComponentBase } from '@shared/common/app-component-base';
import { AppUiCustomizationService } from '@shared/common/ui/app-ui-customization.service';

import * as moment from 'moment';

import { LoginService } from './login/login.service';

@Component({
  standalone: false,
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.less'],
  encapsulation: ViewEncapsulation.None,
})
export class AccountComponent extends AppComponentBase implements OnInit {
  private readonly viewContainerRef: ViewContainerRef;

  currentLanguage: eaf.localization.ILanguageInfo;
  languages: eaf.localization.ILanguageInfo[] = [];

  currentYear: number = moment().year();
  remoteServiceBaseUrl: string = AppConsts.remoteServiceBaseUrl;

  public constructor(
    injector: Injector,
    private readonly _router: Router,
    private readonly _loginService: LoginService,
    private readonly _uiCustomizationService: AppUiCustomizationService,
    viewContainerRef: ViewContainerRef,
    private readonly _storageService: StorageService,
  ) {
    super(injector);

    // We need this small hack in order to catch application root view container ref for modals
    this.viewContainerRef = viewContainerRef;
  }

  showTenantChange(): boolean {
    if (!this._router.url) {
      return false;
    }

    return eaf.multiTenancy.isEnabled;
  }

  useFullWidthLayout(): boolean {
    return false;
  }

  ngOnInit(): void {
    document.body.className = this._uiCustomizationService.getAccountModuleBodyClass();

    this.languages = eaf.localization.languages?.filter(l => (<any>l).isDisabled === false) || [];
    this.currentLanguage = eaf.localization.currentLanguage;
  }

  isForgotPassword(): boolean {
    if (this._router.url == '/account/forgot-password') {
      return true;
    } else {
      return false;
    }
  }

  goToHome(): void {
    (window as any).location.href = '/';
  }

  getBgUrl(): string {
    return 'url(./assets/common/images/login_bg.jpg)';
  }

  changeLanguage(language: eaf.localization.ILanguageInfo): void {
    this._storageService.setCookieValue(
      'Abp.Localization.CultureName',
      language.name,
      new Date(Date.now() + 5 * 365 * 86400000), // 5 year
      eaf.appPath,
    );

    location.reload();
  }

  getInitials(languageName: string) {
    return AppConsts.LocaleCurrency.find(l => l.locale == languageName).initials;
  }
}
